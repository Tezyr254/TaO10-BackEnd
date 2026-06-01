using TaO10_BackEnd.DTOs.Exams;

namespace TaO10_BackEnd.Services;

/// <summary>
/// Interface for exam-related services (read-only)
/// </summary>
public interface IExamService
{
    /// <summary>
    /// Gets an exam by ID without questions
    /// </summary>
    Task<ExamDto> GetExamByIdAsync(Guid examId);

    /// <summary>
    /// Gets all active exams with pagination
    /// </summary>
    Task<(List<ExamDto> Exams, int TotalCount)> GetAllExamsAsync(int pageNumber = 1, int pageSize = 10);

    /// <summary>
    /// Gets an exam with all questions (without correct answers)
    /// </summary>
    Task<ExamResponseDto> GetExamWithQuestionsAsync(Guid examId);
}
