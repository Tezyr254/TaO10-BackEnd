using TaO10_BackEnd.DTOs.Exams;

namespace TaO10_BackEnd.Services;

/// <summary>
/// Interface for user exam attempt services
/// </summary>
public interface IUserExamAttemptService
{
    /// <summary>
    /// Starts a new exam attempt
    /// </summary>
    Task<UserExamAttemptDto> StartExamAsync(StartExamRequest request);

    /// <summary>
    /// Gets an attempt by ID
    /// </summary>
    Task<UserExamAttemptDto> GetAttemptByIdAsync(Guid attemptId);

    /// <summary>
    /// Gets all attempts for a user
    /// </summary>
    Task<(List<UserExamAttemptDto> Attempts, int TotalCount)> GetUserAttemptsAsync(Guid userId, int pageNumber = 1, int pageSize = 10);

    /// <summary>
    /// Submits an answer for a question
    /// </summary>
    Task<UserAnswerDto> SubmitAnswerAsync(Guid attemptId, SubmitAnswerRequest request);

    /// <summary>
    /// Submits the exam (completes the attempt)
    /// </summary>
    Task<UserExamAttemptDto> SubmitExamAsync(Guid attemptId, SubmitExamRequest request);
}
