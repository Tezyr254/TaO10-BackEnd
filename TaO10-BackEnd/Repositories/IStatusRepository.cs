using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Repositories;

/// <summary>
/// Interface for Status repository
/// </summary>
public interface IStatusRepository : IRepository<Status>
{
    /// <summary>
    /// Finds a status by entity type and code
    /// </summary>
    Task<Status?> FindByEntityTypeAndCodeAsync(string entityType, string code);
}
