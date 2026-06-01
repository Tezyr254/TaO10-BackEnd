using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Repositories;

/// <summary>
/// Implementation of Status repository
/// </summary>
public class StatusRepository : Repository<Status>, IStatusRepository
{
    /// <summary>
    /// Initializes a new instance of the StatusRepository class
    /// </summary>
    public StatusRepository(AppDbContext context) : base(context) { }

    /// <summary>
    /// Finds a status by entity type and code
    /// </summary>
    public async Task<Status?> FindByEntityTypeAndCodeAsync(string entityType, string code)
    {
        return await _dbSet
            .Where(s => s.EntityType == entityType && s.Code == code)
            .FirstOrDefaultAsync();
    }
}
