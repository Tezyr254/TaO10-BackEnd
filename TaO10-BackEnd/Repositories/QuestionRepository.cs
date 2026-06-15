using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Repositories;

/// <summary>
/// Implementation of Question repository
/// </summary>
public class QuestionRepository : Repository<Question>, IQuestionRepository
{
    /// <summary>
    /// Initializes a new instance of the QuestionRepository class
    /// </summary>
    public QuestionRepository(AppDbContext context) : base(context) { }

    /// <summary>
    /// Gets all questions for an exam
    /// </summary>
    public async Task<List<Question>> GetByExamAsync(Guid examId)
    {
        return await _dbSet
            .Include(q => q.Status)
            .Where(q => q.ExamId == examId)
            .OrderBy(q => q.QuestionNumber)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a question with status information
    /// </summary>
    public async Task<Question?> GetByIdWithStatusAsync(Guid questionId)
    {
        return await _dbSet
            .Include(q => q.Status)
            .FirstOrDefaultAsync(q => q.QuestionId == questionId);
    }

    /// <summary>
    /// Gets a question with exam information
    /// </summary>
    public async Task<Question?> GetByIdWithExamAsync(Guid questionId)
    {
        return await _dbSet
            .Include(q => q.Exam)
            .Include(q => q.Status)
            .FirstOrDefaultAsync(q => q.QuestionId == questionId);
    }
}
