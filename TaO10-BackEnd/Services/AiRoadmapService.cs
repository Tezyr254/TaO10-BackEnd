using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.Common;
using TaO10_BackEnd.DTOs.AiRoadmaps;
using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Services;

public class AiRoadmapService : IAiRoadmapService
{
    private readonly AppDbContext _dbContext;
    private readonly IGeminiRoadmapService _geminiRoadmapService;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public AiRoadmapService(
        AppDbContext dbContext,
        IGeminiRoadmapService geminiRoadmapService)
    {
        _dbContext = dbContext;
        _geminiRoadmapService = geminiRoadmapService;
    }

    public async Task<StudyRoadmapDto?> GetRoadmapAsync(Guid userId)
    {
        var roadmap = await GetExistingRoadmapAsync(userId);
        return roadmap == null ? null : MapToDto(roadmap);
    }

    public async Task<StudyRoadmapDto> GenerateRoadmapAsync(Guid userId)
    {
        var attempt = await GetLatestCompletedAttemptAsync(userId);
        if (attempt == null || attempt.UserAnswers.Count == 0)
        {
            throw new InvalidOperationException("Bạn cần làm bài ít nhất 1 lần để có dữ liệu phân tích");
        }

        var existing = await GetExistingRoadmapAsync(userId);
        if (existing?.UserExamAttemptId == attempt.UserExamAttemptId)
        {
            return MapToDto(existing);
        }

        var generated = await _geminiRoadmapService.GenerateRoadmapAsync(attempt);
        var roadmap = existing ?? CreateRoadmap(userId);
        ApplyGeneratedRoadmap(roadmap, attempt, generated);

        await _dbContext.SaveChangesAsync();

        roadmap.UserExamAttempt = attempt;
        return MapToDto(roadmap);
    }

    private async Task<UserStudyRoadmap?> GetExistingRoadmapAsync(Guid userId)
    {
        return await _dbContext.UserStudyRoadmaps
            .Include(item => item.UserExamAttempt)
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.UpdatedAt)
            .FirstOrDefaultAsync();
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

    private UserStudyRoadmap CreateRoadmap(Guid userId)
    {
        var roadmap = new UserStudyRoadmap
        {
            UserStudyRoadmapId = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.UserStudyRoadmaps.Add(roadmap);
        return roadmap;
    }

    private void ApplyGeneratedRoadmap(
        UserStudyRoadmap roadmap,
        UserExamAttempt attempt,
        GeneratedRoadmap generated)
    {
        roadmap.UserExamAttemptId = attempt.UserExamAttemptId;
        roadmap.Summary = generated.Summary;
        roadmap.Strengths = JsonSerializer.Serialize(generated.Strengths, _jsonOptions);
        roadmap.Weaknesses = JsonSerializer.Serialize(generated.Weaknesses, _jsonOptions);
        roadmap.Weeks = JsonSerializer.Serialize(
            generated.Weeks.Select(week => new StudyRoadmapWeekDto
            {
                Title = week.Title,
                Goal = week.Goal,
                Tasks = week.Tasks
            }).ToList(),
            _jsonOptions);
        roadmap.DailyTime = generated.DailyTime;
        roadmap.NextAction = generated.NextAction;
        roadmap.UpdatedAt = DateTime.UtcNow;
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
}
