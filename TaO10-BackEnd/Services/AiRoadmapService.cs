using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.Common;
using TaO10_BackEnd.DTOs.AiRoadmaps;
using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Services;

public class AiRoadmapService : IAiRoadmapService
{
    private readonly AppDbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiRoadmapService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public AiRoadmapService(
        AppDbContext dbContext,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AiRoadmapService> logger)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<StudyRoadmapDto?> GetRoadmapAsync(Guid userId)
    {
        var roadmap = await _dbContext.UserStudyRoadmaps
            .Include(item => item.UserExamAttempt)
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.UpdatedAt)
            .FirstOrDefaultAsync();

        return roadmap == null ? null : MapToDto(roadmap);
    }

    public async Task<StudyRoadmapDto> GenerateRoadmapAsync(Guid userId)
    {
        var attempt = await GetLatestCompletedAttemptAsync(userId);
        if (attempt == null)
        {
            throw new InvalidOperationException("Bạn cần làm bài ít nhất 1 lần để có dữ liệu phân tích");
        }

        if (attempt.UserAnswers.Count == 0)
        {
            throw new InvalidOperationException("Bạn cần làm bài ít nhất 1 lần để có dữ liệu phân tích");
        }

        var generated = await TryGenerateWithGeminiAsync(attempt);
        var existing = await _dbContext.UserStudyRoadmaps.FirstOrDefaultAsync(item => item.UserId == userId);
        var now = DateTime.UtcNow;

        if (existing == null)
        {
            existing = new UserStudyRoadmap
            {
                UserStudyRoadmapId = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = now
            };
            _dbContext.UserStudyRoadmaps.Add(existing);
        }

        existing.UserExamAttemptId = attempt.UserExamAttemptId;
        existing.Summary = generated.Summary;
        existing.Strengths = JsonSerializer.Serialize(generated.Strengths, _jsonOptions);
        existing.Weaknesses = JsonSerializer.Serialize(generated.Weaknesses, _jsonOptions);
        existing.Weeks = JsonSerializer.Serialize(generated.Weeks, _jsonOptions);
        existing.DailyTime = generated.DailyTime;
        existing.NextAction = generated.NextAction;
        existing.UpdatedAt = now;

        await _dbContext.SaveChangesAsync();

        existing.UserExamAttempt = attempt;
        return MapToDto(existing);
    }

    private async Task<UserExamAttempt?> GetLatestCompletedAttemptAsync(Guid userId)
    {
        return await _dbContext.UserExamAttempts
            .Include(attempt => attempt.Exam)
                .ThenInclude(exam => exam!.Questions)
            .Include(attempt => attempt.Status)
            .Include(attempt => attempt.UserAnswers)
                .ThenInclude(answer => answer.Question)
            .Where(attempt =>
                attempt.UserId == userId &&
                attempt.CompletedAt != null &&
                attempt.Status.Code == AppStatusCodes.Attempts.Submitted)
            .OrderByDescending(attempt => attempt.CompletedAt)
            .FirstOrDefaultAsync();
    }

    private async Task<GeneratedRoadmap> TryGenerateWithGeminiAsync(UserExamAttempt attempt)
    {
        try
        {
            return await GenerateWithGeminiAsync(attempt);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gemini failed, using local roadmap fallback for attempt {AttemptId}", attempt.UserExamAttemptId);
            return GenerateFallbackRoadmap(attempt);
        }
    }

    private async Task<GeneratedRoadmap> GenerateWithGeminiAsync(UserExamAttempt attempt)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Chưa cấu hình Gemini API key trên backend.");
        }

        var model = _configuration["Gemini:Model"];
        if (string.IsNullOrWhiteSpace(model))
        {
            model = "gemini-1.5-flash";
        }

        var request = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = BuildPrompt(attempt) }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.35,
                responseMimeType = "application/json"
            }
        };

        using var content = new StringContent(JsonSerializer.Serialize(request, _jsonOptions), Encoding.UTF8, "application/json");
        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={Uri.EscapeDataString(apiKey)}";
        using var response = await _httpClient.PostAsync(endpoint, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Gemini roadmap generation failed. Status: {StatusCode}. Body: {Body}", response.StatusCode, responseBody);
            throw new InvalidOperationException("Không gọi được Gemini để tạo lộ trình.");
        }

        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseBody, _jsonOptions);
        var text = string.Join(
            string.Empty,
            geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.Select(part => part.Text ?? string.Empty) ?? Array.Empty<string>());

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Gemini không trả về nội dung lộ trình.");
        }

        return ParseGeneratedRoadmap(text);
    }

    private GeneratedRoadmap GenerateFallbackRoadmap(UserExamAttempt attempt)
    {
        var totalQuestions = attempt.TotalQuestions ?? attempt.Exam?.QuestionsCount ?? attempt.Exam?.Questions.Count ?? 0;
        var correctAnswers = attempt.CorrectAnswers ?? attempt.UserAnswers.Count(answer => answer.IsCorrect == true);
        var skipped = Math.Max(0, totalQuestions - attempt.UserAnswers.Count);
        var wrongAnswers = attempt.UserAnswers.Where(answer => answer.IsCorrect != true).ToList();
        var weakSections = wrongAnswers
            .Select(answer => answer.Question?.Section)
            .Where(section => !string.IsNullOrWhiteSpace(section))
            .GroupBy(section => section!)
            .OrderByDescending(group => group.Count())
            .Select(group => group.Key)
            .Take(3)
            .ToList();

        if (weakSections.Count == 0)
        {
            weakSections.AddRange(new[] { "Ngữ pháp và từ vựng", "Đọc hiểu", "Viết lại câu" });
        }

        var score = attempt.Score.HasValue ? Math.Round(attempt.Score.Value / 10, 2) : 0;
        var summary = $"Bạn đạt khoảng {score}/10 với {correctAnswers}/{totalQuestions} câu đúng. Lộ trình này ưu tiên xử lý các phần còn yếu từ attempt gần nhất, đặc biệt là {string.Join(", ", weakSections)}.";

        return new GeneratedRoadmap
        {
            Summary = summary,
            Strengths = new List<string>
            {
                correctAnswers > totalQuestions / 2 ? "Đã có nền tảng làm bài tương đối ổn" : "Đã có dữ liệu bài làm để xác định điểm yếu",
                "Có thể cải thiện nhanh nếu ôn theo nhóm lỗi sai"
            },
            Weaknesses = weakSections.Concat(skipped > 0 ? new[] { "Quản lý thời gian và câu bỏ qua" } : Array.Empty<string>()).Take(5).ToList(),
            Weeks = new List<StudyRoadmapWeekDto>
            {
                new()
                {
                    Title = "Tuần 1",
                    Goal = $"Củng cố {weakSections[0]}",
                    Tasks = new List<string>
                    {
                        "Xem lại toàn bộ câu sai trong attempt gần nhất và ghi lý do sai",
                        $"Ôn lý thuyết trọng tâm của phần {weakSections[0]}",
                        "Làm 20-30 câu luyện tập cùng chủ điểm và sửa lỗi ngay sau khi làm"
                    }
                },
                new()
                {
                    Title = "Tuần 2",
                    Goal = weakSections.Count > 1 ? $"Tăng điểm phần {weakSections[1]}" : "Tăng tốc luyện đề theo nhóm lỗi",
                    Tasks = new List<string>
                    {
                        "Làm bài luyện theo thời gian giới hạn 25-30 phút",
                        "Tổng hợp các bẫy đáp án hay chọn nhầm",
                        "Ôn lại từ vựng/cấu trúc xuất hiện trong câu sai"
                    }
                },
                new()
                {
                    Title = "Tuần 3",
                    Goal = "Thi thử và đo tiến bộ",
                    Tasks = new List<string>
                    {
                        "Làm ít nhất 2 đề hoàn chỉnh trong đúng thời gian",
                        "So sánh lỗi mới với attempt cũ để xem phần nào đã giảm lỗi",
                        "Tập trung ôn 3 nhóm lỗi còn lặp lại nhiều nhất"
                    }
                }
            },
            DailyTime = "30-45 phút/ngày",
            NextAction = "Làm lại một đề sau 3-5 ngày ôn để cập nhật lộ trình."
        };
    }

    private string BuildPrompt(UserExamAttempt attempt)
    {
        var totalQuestions = attempt.TotalQuestions ?? attempt.Exam?.QuestionsCount ?? attempt.Exam?.Questions.Count ?? 0;
        var correctAnswers = attempt.CorrectAnswers ?? attempt.UserAnswers.Count(answer => answer.IsCorrect == true);
        var skipped = Math.Max(0, totalQuestions - attempt.UserAnswers.Count);
        var wrong = Math.Max(0, totalQuestions - correctAnswers - skipped);
        var score = attempt.Score.HasValue ? Math.Round(attempt.Score.Value / 10, 2) : 0;

        var wrongQuestions = attempt.UserAnswers
            .Where(answer => answer.IsCorrect != true)
            .OrderBy(answer => answer.Question?.QuestionNumber)
            .Select(answer => new
            {
                questionNumber = answer.Question?.QuestionNumber,
                section = (string?)answer.Question?.Section,
                question = (string?)answer.Question?.QuestionText,
                userAnswer = (string?)answer.UserAnswer1?.ToString(),
                correctAnswer = (string?)answer.Question?.CorrectAnswer,
                explanation = (string?)answer.Question?.Explanation
            })
            .ToList();

        var skippedQuestions = (attempt.Exam?.Questions ?? new List<Question>())
            .Where(question => attempt.UserAnswers.All(answer => answer.QuestionId != question.QuestionId))
            .OrderBy(question => question.QuestionNumber)
            .Select(question => new
            {
                questionNumber = (int?)question.QuestionNumber,
                section = (string?)question.Section,
                question = (string?)question.QuestionText,
                userAnswer = (string?)string.Empty,
                correctAnswer = (string?)question.CorrectAnswer,
                explanation = (string?)question.Explanation
            })
            .ToList();

        var attemptData = new
        {
            examTitle = attempt.Exam?.Title,
            completedAt = attempt.CompletedAt,
            score,
            correct = correctAnswers,
            wrong,
            skip = skipped,
            totalQuestions,
            weakQuestions = wrongQuestions.Concat(skippedQuestions)
        };

        return $$"""
Bạn là giáo viên tiếng Anh luyện thi vào lớp 10. Hãy phân tích attempt gần nhất của học viên và tạo lộ trình học cá nhân hóa trong 3 tuần.

Yêu cầu trả về DUY NHẤT JSON hợp lệ, không markdown, theo schema:
{
  "summary": "nhận xét tổng quan 2-3 câu bằng tiếng Việt",
  "strengths": ["điểm mạnh 1", "điểm mạnh 2"],
  "weaknesses": ["điểm yếu 1", "điểm yếu 2", "điểm yếu 3"],
  "weeks": [
    { "title": "Tuần 1", "goal": "mục tiêu", "tasks": ["việc cần làm", "việc cần làm", "việc cần làm"] },
    { "title": "Tuần 2", "goal": "mục tiêu", "tasks": ["việc cần làm", "việc cần làm", "việc cần làm"] },
    { "title": "Tuần 3", "goal": "mục tiêu", "tasks": ["việc cần làm", "việc cần làm", "việc cần làm"] }
  ],
  "dailyTime": "thời lượng học mỗi ngày",
  "nextAction": "hành động tiếp theo ngắn gọn"
}

Dữ liệu attempt:
{{JsonSerializer.Serialize(attemptData, _jsonOptions)}}
""";
    }

    private GeneratedRoadmap ParseGeneratedRoadmap(string text)
    {
        var cleaned = text.Trim();
        if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned[7..].Trim();
        }
        else if (cleaned.StartsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned[3..].Trim();
        }

        if (cleaned.EndsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned[..^3].Trim();
        }

        var roadmap = JsonSerializer.Deserialize<GeneratedRoadmap>(cleaned, _jsonOptions);
        if (roadmap == null)
        {
            throw new InvalidOperationException("Không đọc được JSON lộ trình từ Gemini.");
        }

        roadmap.Strengths = roadmap.Strengths.Where(item => !string.IsNullOrWhiteSpace(item)).Take(4).ToList();
        roadmap.Weaknesses = roadmap.Weaknesses.Where(item => !string.IsNullOrWhiteSpace(item)).Take(5).ToList();
        roadmap.Weeks = roadmap.Weeks.Take(3).Select((week, index) => new StudyRoadmapWeekDto
        {
            Title = string.IsNullOrWhiteSpace(week.Title) ? $"Tuần {index + 1}" : week.Title,
            Goal = week.Goal ?? string.Empty,
            Tasks = week.Tasks.Where(item => !string.IsNullOrWhiteSpace(item)).Take(5).ToList()
        }).ToList();
        roadmap.DailyTime = string.IsNullOrWhiteSpace(roadmap.DailyTime) ? "30-45 phút/ngày" : roadmap.DailyTime;
        roadmap.NextAction = string.IsNullOrWhiteSpace(roadmap.NextAction) ? "Làm lại bài và theo dõi lỗi sai." : roadmap.NextAction;

        return roadmap;
    }

    private StudyRoadmapDto MapToDto(UserStudyRoadmap roadmap)
    {
        return new StudyRoadmapDto
        {
            UserStudyRoadmapId = roadmap.UserStudyRoadmapId,
            SourceAttemptId = roadmap.UserExamAttemptId,
            SourceSubmittedAt = roadmap.UserExamAttempt?.CompletedAt,
            CreatedAt = roadmap.CreatedAt,
            Summary = roadmap.Summary,
            Strengths = DeserializeList(roadmap.Strengths),
            Weaknesses = DeserializeList(roadmap.Weaknesses),
            Weeks = DeserializeWeeks(roadmap.Weeks),
            DailyTime = roadmap.DailyTime,
            NextAction = roadmap.NextAction
        };
    }

    private List<string> DeserializeList(string value)
    {
        return JsonSerializer.Deserialize<List<string>>(value, _jsonOptions) ?? new List<string>();
    }

    private List<StudyRoadmapWeekDto> DeserializeWeeks(string value)
    {
        return JsonSerializer.Deserialize<List<StudyRoadmapWeekDto>>(value, _jsonOptions) ?? new List<StudyRoadmapWeekDto>();
    }

    private sealed class GeneratedRoadmap
    {
        public string Summary { get; set; } = string.Empty;

        public List<string> Strengths { get; set; } = new();

        public List<string> Weaknesses { get; set; } = new();

        public List<StudyRoadmapWeekDto> Weeks { get; set; } = new();

        public string DailyTime { get; set; } = string.Empty;

        public string NextAction { get; set; } = string.Empty;
    }

    private sealed class GeminiResponse
    {
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private sealed class GeminiCandidate
    {
        public GeminiContent? Content { get; set; }
    }

    private sealed class GeminiContent
    {
        public List<GeminiPart>? Parts { get; set; }
    }

    private sealed class GeminiPart
    {
        public string? Text { get; set; }
    }
}
