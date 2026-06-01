namespace TaO10_BackEnd.Repositories;

/// <summary>
/// Generic repository interface for all entities
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets an entity by ID
    /// </summary>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Adds an entity
    /// </summary>
    Task AddAsync(T entity);

    /// <summary>
    /// Updates an entity
    /// </summary>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Deletes an entity
    /// </summary>
    Task DeleteAsync(T entity);

    /// <summary>
    /// Saves changes to the database
    /// </summary>
    Task SaveChangesAsync();
}
