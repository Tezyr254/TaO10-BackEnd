using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Repositories;

/// <summary>
/// Interface for UserExamAttempt repository
/// </summary>
public interface IUserExamAttemptRepository : IRepository<UserExamAttempt>
{
    /// <summary>
    /// Gets all attempts for a user with pagination
    /// </summary>
    Task<(List<UserExamAttempt> Attempts, int TotalCount)> GetUserAttemptsAsync(Guid userId, int pageNumber = 1, int pageSize = 10);

    /// <summary>
    /// Gets an attempt with all related data (exam, questions, answers)
    /// </summary>
    Task<UserExamAttempt?> GetByIdWithAnswersAsync(Guid attemptId);

    /// <summary>
    /// Checks if an attempt is currently in progress
    /// </summary>
    Task<bool> IsAttemptInProgressAsync(Guid attemptId);

    /// <summary>
    /// Gets an attempt with status information
    /// </summary>
    Task<UserExamAttempt?> GetByIdWithStatusAsync(Guid attemptId);

    /// <summary>
    /// Gets an attempt with exam, status, and answers
    /// </summary>
    Task<UserExamAttempt?> GetByIdFullAsync(Guid attemptId);
}
