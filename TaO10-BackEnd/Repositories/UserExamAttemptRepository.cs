using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Repositories;

/// <summary>
/// Implementation of UserExamAttempt repository
/// </summary>
public class UserExamAttemptRepository : Repository<UserExamAttempt>, IUserExamAttemptRepository
{
    /// <summary>
    /// Initializes a new instance of the UserExamAttemptRepository class
    /// </summary>
    public UserExamAttemptRepository(AppDbContext context) : base(context) { }

    /// <summary>
    /// Gets all attempts for a user with pagination
    /// </summary>
    public async Task<(List<UserExamAttempt> Attempts, int TotalCount)> GetUserAttemptsAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
    {
        var query = _dbSet
            .Include(ua => ua.Exam)
            .Include(ua => ua.Status)
            .Where(ua => ua.UserId == userId)
            .OrderByDescending(ua => ua.StartedAt);

        int totalCount = await query.CountAsync();
        var attempts = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (attempts, totalCount);
    }

    /// <summary>
    /// Gets an attempt with all related data (exam, questions, answers)
    /// </summary>
    public async Task<UserExamAttempt?> GetByIdWithAnswersAsync(Guid attemptId)
    {
        return await _dbSet
            .Include(ua => ua.Status)
            .Include(ua => ua.Exam)
            .Include(ua => ua.UserAnswers)
            .FirstOrDefaultAsync(ua => ua.UserExamAttemptId == attemptId);
    }

    /// <summary>
    /// Checks if an attempt is currently in progress
    /// </summary>
    public async Task<bool> IsAttemptInProgressAsync(Guid attemptId)
    {
        return await _dbSet
            .Include(ua => ua.Status)
            .AnyAsync(ua => ua.UserExamAttemptId == attemptId && ua.Status.Code == "in_progress");
    }

    /// <summary>
    /// Gets an attempt with status information
    /// </summary>
    public async Task<UserExamAttempt?> GetByIdWithStatusAsync(Guid attemptId)
    {
        return await _dbSet
            .Include(ua => ua.Status)
            .FirstOrDefaultAsync(ua => ua.UserExamAttemptId == attemptId);
    }

    /// <summary>
    /// Gets an attempt with exam, status, and answers
    /// </summary>
    public async Task<UserExamAttempt?> GetByIdFullAsync(Guid attemptId)
    {
        return await _dbSet
            .Include(ua => ua.Status)
            .Include(ua => ua.Exam)
            .Include(ua => ua.UserAnswers)
                .ThenInclude(ua => ua.Question)
            .FirstOrDefaultAsync(ua => ua.UserExamAttemptId == attemptId);
    }
}
