using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.DTOs.Attempt;
using TaO10_BackEnd.DTOs.Exam;
using TaO10_BackEnd.Hubs;
using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttemptsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AttemptsController(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // POST: api/Attempts/exam/{examId}/start
        [Authorize]
        [HttpPost("exam/{examId}/start")]
        public async Task<ActionResult<StartAttemptResponse>> StartAttempt(Guid examId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var exam = await _context.Exams
                .Include(e => e.PackageExams)
                .FirstOrDefaultAsync(e => e.ExamId == examId);

            if (exam == null) return NotFound("Đề thi không tồn tại.");

            // Check if user has package access for this exam
            var allowedPackageIds = exam.PackageExams.Select(pe => pe.PackageId).ToList();
            if (allowedPackageIds.Any())
            {
                var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
                var isAdmin = roleClaim != null && roleClaim.Value == "admin";

                if (!isAdmin)
                {
                    var activePkgStatus = await _context.Statuses
                        .FirstOrDefaultAsync(s => s.EntityType == "UserPackage" && s.Code == "ACTIVE");

                    var hasAccess = await _context.UserPackages
                        .AnyAsync(up => up.UserId == userId &&
                                        up.StatusId == (activePkgStatus != null ? activePkgStatus.StatusId : Guid.Empty) &&
                                        allowedPackageIds.Contains(up.PackageId ?? Guid.Empty) &&
                                        (up.EndDate == null || up.EndDate >= DateTime.UtcNow));

                    if (!hasAccess)
                    {
                        return StatusCode(403, new { message = "Đề thi này không thuộc gói dịch vụ hiện tại của bạn. Vui lòng nâng cấp gói để bắt đầu làm bài." });
                    }
                }
            }

            var activeAttemptStatus = await _context.Statuses
                .FirstOrDefaultAsync(s => s.EntityType == "UserExamAttempt" && s.Code == "ACTIVE"); // Or we can use code "IN_PROGRESS"

            if (activeAttemptStatus == null)
            {
                // Fallback to active status
                activeAttemptStatus = await _context.Statuses
                    .FirstOrDefaultAsync(s => s.EntityType == "UserExamAttempt");
            }

            // Create new attempt
            var attempt = new UserExamAttempt
            {
                UserExamAttemptId = Guid.NewGuid(),
                UserId = userId,
                ExamId = examId,
                StartedAt = DateTime.UtcNow,
                StatusId = activeAttemptStatus?.StatusId ?? Guid.Empty
            };

            _context.UserExamAttempts.Add(attempt);

            // Increment views count of exam
            exam.ViewsCount = (exam.ViewsCount ?? 0) + 1;

            // Log user progress as started (10%)
            var progressStatus = await _context.UserProgresses
                .FirstOrDefaultAsync(up => up.UserId == userId && up.ItemType == "EXAM" && up.ItemId == examId);

            if (progressStatus == null)
            {
                progressStatus = new UserProgress
                {
                    UserProgressId = Guid.NewGuid(),
                    UserId = userId,
                    ItemType = "EXAM",
                    ItemId = examId,
                    ProgressPercentage = 10,
                    LastAccessed = DateTime.UtcNow
                };
                _context.UserProgresses.Add(progressStatus);
            }
            else
            {
                progressStatus.LastAccessed = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Load questions (hiding answers)
            var activeQuestionStatus = await _context.Statuses
                .FirstOrDefaultAsync(s => s.EntityType == "Question" && s.Code == "ACTIVE");

            var qQuery = _context.Questions.Where(q => q.ExamId == examId);
            if (activeQuestionStatus != null)
            {
                qQuery = qQuery.Where(q => q.StatusId == activeQuestionStatus.StatusId);
            }

            var questions = await qQuery
                .OrderBy(q => q.QuestionNumber)
                .Select(q => new QuestionResponse
                {
                    QuestionId = q.QuestionId,
                    QuestionNumber = q.QuestionNumber,
                    Section = q.Section,
                    QuestionText = q.QuestionText,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD,
                    Points = q.Points
                })
                .ToListAsync();

            return Ok(new StartAttemptResponse
            {
                UserExamAttemptId = attempt.UserExamAttemptId,
                ExamTitle = exam.Title,
                DurationTime = exam.DurationTime,
                Questions = questions
            });
        }

        // POST: api/Attempts/{attemptId}/answers (Save draft answer)
        [Authorize]
        [HttpPost("{attemptId}/answers")]
        public async Task<IActionResult> SaveDraftAnswer(Guid attemptId, [FromBody] SaveAnswerRequest request)
        {
            var attempt = await _context.UserExamAttempts.FindAsync(attemptId);
            if (attempt == null) return NotFound("Lượt thi không tồn tại.");

            // Check if attempt is still active (not completed yet)
            var successStatus = await _context.Statuses
                .FirstOrDefaultAsync(s => s.EntityType == "UserExamAttempt" && s.Code == "COMPLETED");
            if (successStatus != null && attempt.StatusId == successStatus.StatusId)
            {
                return BadRequest("Bài thi này đã được nộp và chấm điểm.");
            }

            var existingAnswer = await _context.UserAnswers
                .FirstOrDefaultAsync(ua => ua.UserExamAttemptId == attemptId && ua.QuestionId == request.QuestionId);

            if (existingAnswer != null)
            {
                existingAnswer.UserAnswer1 = !string.IsNullOrEmpty(request.UserAnswer) ? request.UserAnswer[0] : null;
                existingAnswer.AnsweredAt = DateTime.UtcNow;
            }
            else
            {
                var newAnswer = new UserAnswer
                {
                    UserAnswerId = Guid.NewGuid(),
                    UserExamAttemptId = attemptId,
                    QuestionId = request.QuestionId,
                    UserAnswer1 = !string.IsNullOrEmpty(request.UserAnswer) ? request.UserAnswer[0] : null,
                    AnsweredAt = DateTime.UtcNow
                };
                _context.UserAnswers.Add(newAnswer);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // POST: api/Attempts/{attemptId}/submit (Submit and grade exam)
        [Authorize]
        [HttpPost("{attemptId}/submit")]
        public async Task<ActionResult<SubmitAttemptResponse>> SubmitAttempt(Guid attemptId, [FromBody] SubmitAttemptRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var attempt = await _context.UserExamAttempts
                .Include(a => a.Exam)
                .FirstOrDefaultAsync(a => a.UserExamAttemptId == attemptId);

            if (attempt == null) return NotFound("Lượt thi không tồn tại.");
            if (attempt.UserId != userId) return Forbid();

            var completedStatus = await _context.Statuses
                .FirstOrDefaultAsync(s => s.EntityType == "UserExamAttempt" && s.Code == "COMPLETED");

            if (completedStatus == null) return BadRequest("Status COMPLETED for UserExamAttempt not configured.");

            if (attempt.StatusId == completedStatus.StatusId)
            {
                // Attempt already graded, return existing results
                return await GetGradedResult(attemptId);
            }

            // Save incoming answers to DB first (if any)
            foreach (var ans in request.Answers)
            {
                var existing = await _context.UserAnswers
                    .FirstOrDefaultAsync(ua => ua.UserExamAttemptId == attemptId && ua.QuestionId == ans.QuestionId);

                if (existing != null)
                {
                    existing.UserAnswer1 = !string.IsNullOrEmpty(ans.UserAnswer) ? ans.UserAnswer[0] : null;
                    existing.AnsweredAt = DateTime.UtcNow;
                }
                else
                {
                    _context.UserAnswers.Add(new UserAnswer
                    {
                        UserAnswerId = Guid.NewGuid(),
                        UserExamAttemptId = attemptId,
                        QuestionId = ans.QuestionId,
                        UserAnswer1 = !string.IsNullOrEmpty(ans.UserAnswer) ? ans.UserAnswer[0] : null,
                        AnsweredAt = DateTime.UtcNow
                    });
                }
            }
            await _context.SaveChangesAsync();

            // Load all questions for this exam to perform grading
            var questions = await _context.Questions
                .Where(q => q.ExamId == attempt.ExamId)
                .OrderBy(q => q.QuestionNumber)
                .ToListAsync();

            var savedAnswers = await _context.UserAnswers
                .Where(ua => ua.UserExamAttemptId == attemptId)
                .ToDictionaryAsync(ua => ua.QuestionId ?? Guid.Empty, ua => ua);

            int correctCount = 0;
            int totalQuestions = questions.Count;

            var details = new List<QuestionResultDto>();

            foreach (var question in questions)
            {
                savedAnswers.TryGetValue(question.QuestionId, out var userAnswerRecord);
                var userAnswerStr = userAnswerRecord?.UserAnswer1?.ToString();

                bool isCorrect = false;
                if (!string.IsNullOrEmpty(userAnswerStr) && !string.IsNullOrEmpty(question.CorrectAnswer))
                {
                    isCorrect = string.Equals(userAnswerStr.Trim(), question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
                }

                if (isCorrect) correctCount++;

                // Update is_correct flag in DB
                if (userAnswerRecord != null)
                {
                    userAnswerRecord.IsCorrect = isCorrect;
                }
                else
                {
                    // Add empty answer
                    _context.UserAnswers.Add(new UserAnswer
                    {
                        UserAnswerId = Guid.NewGuid(),
                        UserExamAttemptId = attemptId,
                        QuestionId = question.QuestionId,
                        UserAnswer1 = null,
                        IsCorrect = false,
                        AnsweredAt = DateTime.UtcNow
                    });
                }

                details.Add(new QuestionResultDto
                {
                    QuestionId = question.QuestionId,
                    QuestionNumber = question.QuestionNumber,
                    Section = question.Section,
                    QuestionText = question.QuestionText,
                    OptionA = question.OptionA,
                    OptionB = question.OptionB,
                    OptionC = question.OptionC,
                    OptionD = question.OptionD,
                    UserAnswer = userAnswerStr,
                    CorrectAnswer = question.CorrectAnswer,
                    IsCorrect = isCorrect,
                    Explanation = question.Explanation,
                    Points = question.Points
                });
            }

            // Calculate Score on 10-point scale
            decimal score = 0m;
            if (totalQuestions > 0)
            {
                score = Math.Round(((decimal)correctCount / totalQuestions) * 10.0m, 2);
            }

            // Calculate time spent
            var completedAt = DateTime.UtcNow;
            var minutes = (int)Math.Max(1, (completedAt - (attempt.StartedAt ?? completedAt)).TotalMinutes);

            // Update attempt record
            attempt.CompletedAt = completedAt;
            attempt.Score = score;
            attempt.CorrectAnswers = correctCount;
            attempt.TotalQuestions = totalQuestions;
            attempt.TimeSpentMinutes = minutes;
            attempt.StatusId = completedStatus.StatusId;

            // Increment exam attempt count
            if (attempt.Exam != null)
            {
                attempt.Exam.AttemptsCount = (attempt.Exam.AttemptsCount ?? 0) + 1;
            }

            // Update user total scores for leaderboard
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.TotalExams = (user.TotalExams ?? 0) + 1;
                // Add score rounded to nearest integer to database total score
                user.TotalScore = (user.TotalScore ?? 0) + (int)Math.Round(score);
                user.UpdatedAt = DateTime.UtcNow;
            }

            // Update progress to 100%
            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(up => up.UserId == userId && up.ItemType == "EXAM" && up.ItemId == attempt.ExamId);
            if (progress != null)
            {
                progress.ProgressPercentage = 100;
                progress.LastAccessed = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Real-Time broadcast updates to Leaderboard and new attempts ticker over SignalR
            var userNameStr = user?.FullName ?? "Học sinh ẩn danh";
            var examTitleStr = attempt.Exam?.Title ?? "Đề thi";
            
            // Broadcast ticker notification to all active users
            await _hubContext.Clients.All.SendAsync("ReceiveAttemptUpdate", userNameStr, examTitleStr, score.ToString("0.0"));

            // Broadcast Leaderboard updates
            var newLeaderboard = await _context.Users
                .Where(u => u.Role == "student" || u.Role == "user")
                .OrderByDescending(u => u.TotalScore)
                .Take(5)
                .Select(u => new
                {
                    name = u.FullName,
                    score = $"{u.TotalExams} bài",
                    points = u.TotalScore.ToString(),
                    initial = string.IsNullOrEmpty(u.FullName) ? "U" : u.FullName.Substring(0, 1).ToUpper()
                })
                .ToListAsync();
            await _hubContext.Clients.All.SendAsync("LeaderboardUpdated", newLeaderboard);

            return Ok(new SubmitAttemptResponse
            {
                UserExamAttemptId = attemptId,
                Score = score,
                CorrectAnswers = correctCount,
                TotalQuestions = totalQuestions,
                TimeSpentMinutes = minutes,
                Details = details
            });
        }

        // GET: api/Attempts/{attemptId}/result
        [Authorize]
        [HttpGet("{attemptId}/result")]
        public async Task<ActionResult<SubmitAttemptResponse>> GetAttemptResult(Guid attemptId)
        {
            return await GetGradedResult(attemptId);
        }

        // GET: api/Attempts/my-history
        [Authorize]
        [HttpGet("my-history")]
        public async Task<ActionResult<IEnumerable<AttemptHistoryResponse>>> GetMyHistory()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var history = await _context.UserExamAttempts
                .Include(a => a.Exam)
                .Include(a => a.Status)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.StartedAt)
                .Select(a => new AttemptHistoryResponse
                {
                    UserExamAttemptId = a.UserExamAttemptId,
                    ExamId = a.ExamId ?? Guid.Empty,
                    ExamTitle = a.Exam != null ? a.Exam.Title : "Đề thi đã xóa",
                    StartedAt = a.StartedAt ?? DateTime.UtcNow,
                    CompletedAt = a.CompletedAt,
                    Score = a.Score,
                    CorrectAnswers = a.CorrectAnswers,
                    TotalQuestions = a.TotalQuestions,
                    TimeSpentMinutes = a.TimeSpentMinutes,
                    Status = a.Status.DisplayName ?? a.Status.Code
                })
                .ToListAsync();

            return Ok(history);
        }

        // Helper to retrieve graded results safely
        private async Task<ActionResult<SubmitAttemptResponse>> GetGradedResult(Guid attemptId)
        {
            var attempt = await _context.UserExamAttempts
                .Include(a => a.Exam)
                .FirstOrDefaultAsync(a => a.UserExamAttemptId == attemptId);

            if (attempt == null) return NotFound("Lượt thi không tồn tại.");

            var answers = await _context.UserAnswers
                .Include(ua => ua.Question)
                .Where(ua => ua.UserExamAttemptId == attemptId)
                .OrderBy(ua => ua.Question != null ? ua.Question.QuestionNumber : 0)
                .ToListAsync();

            var details = answers.Select(ua => new QuestionResultDto
            {
                QuestionId = ua.QuestionId ?? Guid.Empty,
                QuestionNumber = ua.Question != null ? ua.Question.QuestionNumber : 0,
                Section = ua.Question?.Section,
                QuestionText = ua.Question?.QuestionText ?? "Câu hỏi đã bị xóa",
                OptionA = ua.Question?.OptionA,
                OptionB = ua.Question?.OptionB,
                OptionC = ua.Question?.OptionC,
                OptionD = ua.Question?.OptionD,
                UserAnswer = ua.UserAnswer1?.ToString(),
                CorrectAnswer = ua.Question?.CorrectAnswer,
                IsCorrect = ua.IsCorrect ?? false,
                Explanation = ua.Question?.Explanation,
                Points = ua.Question?.Points
            }).ToList();

            return Ok(new SubmitAttemptResponse
            {
                UserExamAttemptId = attemptId,
                Score = attempt.Score ?? 0m,
                CorrectAnswers = attempt.CorrectAnswers ?? 0,
                TotalQuestions = attempt.TotalQuestions ?? 0,
                TimeSpentMinutes = attempt.TimeSpentMinutes ?? 0,
                Details = details
            });
        }
    }
}
