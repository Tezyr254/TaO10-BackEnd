using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Repositories;

/// <summary>
/// Implementation of UserAnswer repository
/// </summary>
public class UserAnswerRepository : Repository<UserAnswer>, IUserAnswerRepository
{
    /// <summary>
    /// Initializes a new instance of the UserAnswerRepository class
    /// </summary>
    public UserAnswerRepository(AppDbContext context) : base(context) { }

    /// <summary>
    /// Finds an answer by attempt and question
    /// </summary>
    public async Task<UserAnswer?> FindByAttemptAndQuestionAsync(Guid attemptId, Guid questionId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(ua => ua.UserExamAttemptId == attemptId && ua.QuestionId == questionId);
    }

    /// <summary>
    /// Gets all answers for an attempt
    /// </summary>
    public async Task<List<UserAnswer>> GetAnswersByAttemptAsync(Guid attemptId)
    {
        return await _dbSet
            .Where(ua => ua.UserExamAttemptId == attemptId)
            .OrderBy(ua => ua.AnsweredAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets an answer with question information
    /// </summary>
    public async Task<UserAnswer?> GetByIdWithQuestionAsync(Guid answerId)
    {
        return await _dbSet
            .Include(ua => ua.Question)
            .FirstOrDefaultAsync(ua => ua.UserAnswerId == answerId);
    }
}
