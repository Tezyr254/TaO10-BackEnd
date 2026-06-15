using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Repositories;

/// <summary>
/// Interface for Question repository
/// </summary>
public interface IQuestionRepository : IRepository<Question>
{
    /// <summary>
    /// Gets all questions for an exam
    /// </summary>
    Task<List<Question>> GetByExamAsync(Guid examId);

    /// <summary>
    /// Gets a question with status information
    /// </summary>
    Task<Question?> GetByIdWithStatusAsync(Guid questionId);

    /// <summary>
    /// Gets a question with exam information
    /// </summary>
    Task<Question?> GetByIdWithExamAsync(Guid questionId);
}
