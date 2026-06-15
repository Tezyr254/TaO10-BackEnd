using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Repositories;

/// <summary>
/// Interface for UserAnswer repository
/// </summary>
public interface IUserAnswerRepository : IRepository<UserAnswer>
{
    /// <summary>
    /// Finds an answer by attempt and question
    /// </summary>
    Task<UserAnswer?> FindByAttemptAndQuestionAsync(Guid attemptId, Guid questionId);

    /// <summary>
    /// Gets all answers for an attempt
    /// </summary>
    Task<List<UserAnswer>> GetAnswersByAttemptAsync(Guid attemptId);

    /// <summary>
    /// Gets an answer with question information
    /// </summary>
    Task<UserAnswer?> GetByIdWithQuestionAsync(Guid answerId);
}
