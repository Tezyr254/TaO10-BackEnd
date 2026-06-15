using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Repositories;

/// <summary>
/// Generic repository implementation
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// Initializes a new instance of the Repository class
    /// </summary>
    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// Gets an entity by ID
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Gets all entities
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <summary>
    /// Adds an entity
    /// </summary>
    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    /// <summary>
    /// Updates an entity
    /// </summary>
    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Deletes an entity
    /// </summary>
    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Saves changes to the database
    /// </summary>
    public virtual async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
