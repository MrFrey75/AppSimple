namespace AppSimple.Core.Interfaces;

/// <summary>
/// Generic repository interface providing standard CRUD operations for all entities.
/// </summary>
/// <typeparam name="T">The entity type managed by the repository.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>Gets an entity by its unique identifier.</summary>
    /// <param name="uid">The GUID of the entity.</param>
    /// <returns>The entity, or <c>null</c> if not found.</returns>
    Task<T?> GetByUidAsync(Guid uid);

    /// <summary>Returns all entities.</summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>Adds a new entity to the store.</summary>
    Task AddAsync(T entity);

    /// <summary>Updates an existing entity.</summary>
    Task UpdateAsync(T entity);

    /// <summary>Deletes an entity by its unique identifier.</summary>
    Task DeleteAsync(Guid uid);
}
