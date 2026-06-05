using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.DTOs.Exam;
using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuestionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Questions/exam/{examId} (Protected: users can fetch questions to take exams)
        [Authorize]
        [HttpGet("exam/{examId}")]
        public async Task<ActionResult<IEnumerable<QuestionResponse>>> GetQuestionsForExam(Guid examId)
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
                        return StatusCode(403, new { message = "Đề thi này không thuộc gói dịch vụ hiện tại của bạn. Vui lòng nâng cấp gói để xem." });
                    }
                }
            }

            var activeStatus = await _context.Statuses
                .FirstOrDefaultAsync(s => s.EntityType == "Question" && s.Code == "ACTIVE");

            var query = _context.Questions.Where(q => q.ExamId == examId);
            if (activeStatus != null)
            {
                query = query.Where(q => q.StatusId == activeStatus.StatusId);
            }

            var questions = await query
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

            return Ok(questions);
        }

        // GET: api/Questions/admin/exam/{examId} (Admin only: load with answers and explanations)
        [Authorize]
        [HttpGet("admin/exam/{examId}")]
        public async Task<ActionResult<IEnumerable<QuestionAdminResponse>>> GetAdminQuestionsForExam(Guid examId)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "admin")
                return Forbid();

            var questions = await _context.Questions
                .Include(q => q.Status)
                .Where(q => q.ExamId == examId)
                .OrderBy(q => q.QuestionNumber)
                .Select(q => new QuestionAdminResponse
                {
                    QuestionId = q.QuestionId,
                    QuestionNumber = q.QuestionNumber,
                    Section = q.Section,
                    QuestionText = q.QuestionText,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD,
                    Points = q.Points,
                    CorrectAnswer = q.CorrectAnswer,
                    Explanation = q.Explanation,
                    Status = q.Status.DisplayName ?? q.Status.Code
                })
                .ToListAsync();

            return Ok(questions);
        }

        // POST: api/Questions (Admin only)
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<QuestionAdminResponse>> CreateQuestion([FromBody] CreateQuestionRequest request)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "admin")
                return Forbid();

            var exam = await _context.Exams.FindAsync(request.ExamId);
            if (exam == null) return BadRequest("Đề thi liên kết không tồn tại.");

            var activeStatus = await _context.Statuses
                .FirstOrDefaultAsync(s => s.EntityType == "Question" && s.Code == "ACTIVE");

            if (activeStatus == null) return BadRequest("Status 'ACTIVE' for Question not configured.");

            var question = new Question
            {
                QuestionId = Guid.NewGuid(),
                ExamId = request.ExamId,
                QuestionNumber = request.QuestionNumber,
                Section = request.Section,
                QuestionText = request.QuestionText,
                OptionA = request.OptionA,
                OptionB = request.OptionB,
                OptionC = request.OptionC,
                OptionD = request.OptionD,
                CorrectAnswer = request.CorrectAnswer,
                Explanation = request.Explanation,
                Points = request.Points,
                StatusId = activeStatus.StatusId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Questions.Add(question);
            
            // Increment exam questions count
            exam.QuestionsCount = (exam.QuestionsCount ?? 0) + 1;
            exam.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new QuestionAdminResponse
            {
                QuestionId = question.QuestionId,
                QuestionNumber = question.QuestionNumber,
                Section = question.Section,
                QuestionText = question.QuestionText,
                OptionA = question.OptionA,
                OptionB = question.OptionB,
                OptionC = question.OptionC,
                OptionD = question.OptionD,
                Points = question.Points,
                CorrectAnswer = question.CorrectAnswer,
                Explanation = question.Explanation,
                Status = "Active"
            });
        }

        // PUT: api/Questions/{id} (Admin only)
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] UpdateQuestionRequest request)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "admin")
                return Forbid();

            var question = await _context.Questions.FindAsync(id);
            if (question == null) return NotFound("Câu hỏi không tồn tại.");

            var status = await _context.Statuses
                .FirstOrDefaultAsync(s => s.EntityType == "Question" && s.Code == request.Status);

            if (status == null) return BadRequest("Trạng thái không hợp lệ.");

            question.QuestionNumber = request.QuestionNumber;
            question.Section = request.Section;
            question.QuestionText = request.QuestionText;
            question.OptionA = request.OptionA;
            question.OptionB = request.OptionB;
            question.OptionC = request.OptionC;
            question.OptionD = request.OptionD;
            question.CorrectAnswer = request.CorrectAnswer;
            question.Explanation = request.Explanation;
            question.Points = request.Points;
            question.StatusId = status.StatusId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Questions/{id} (Admin only)
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(Guid id)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "admin")
                return Forbid();

            var question = await _context.Questions.FindAsync(id);
            if (question == null) return NotFound("Câu hỏi không tồn tại.");

            var exam = await _context.Exams.FindAsync(question.ExamId);
            if (exam != null)
            {
                exam.QuestionsCount = Math.Max(0, (exam.QuestionsCount ?? 0) - 1);
                exam.UpdatedAt = DateTime.UtcNow;
            }

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
