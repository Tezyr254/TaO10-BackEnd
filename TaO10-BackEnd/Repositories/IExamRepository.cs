using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Repositories;

/// <summary>
/// Interface for Exam repository
/// </summary>
public interface IExamRepository : IRepository<Exam>
{
    /// <summary>
    /// Gets an exam by ID with all related questions
    /// </summary>
    Task<Exam?> GetByIdWithQuestionsAsync(Guid examId);

    /// <summary>
    /// Gets all active exams with pagination
    /// </summary>
    Task<(List<Exam> Exams, int TotalCount)> GetActiveExamsAsync(int pageNumber = 1, int pageSize = 10);

    /// <summary>
    /// Checks if an exam exists
    /// </summary>
    Task<bool> ExamExistsAsync(Guid examId);

    /// <summary>
    /// Gets an exam with status information
    /// </summary>
    Task<Exam?> GetByIdWithStatusAsync(Guid examId);

    /// <summary>
    /// Increments the views count for an exam
    /// </summary>
    Task IncrementViewsCountAsync(Guid examId);

    /// <summary>
    /// Increments the attempts count for an exam
    /// </summary>
    Task IncrementAttemptsCountAsync(Guid examId);
}
