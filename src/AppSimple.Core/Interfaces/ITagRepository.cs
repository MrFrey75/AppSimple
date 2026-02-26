using AppSimple.Core.Models;

namespace AppSimple.Core.Interfaces;

/// <summary>
/// Repository interface for <see cref="Tag"/> entities.
/// Extends the generic repository with tag-specific queries.
/// </summary>
public interface ITagRepository : IRepository<Tag>
{
    /// <summary>Returns all tags belonging to a specific user.</summary>
    /// <param name="userUid">The UID of the owning user.</param>
    Task<IEnumerable<Tag>> GetByUserUidAsync(Guid userUid);

    /// <summary>Finds a tag by name for a given user (case-insensitive).</summary>
    /// <param name="userUid">The UID of the owning user.</param>
    /// <param name="name">The tag name to search for.</param>
    Task<Tag?> GetByNameAsync(Guid userUid, string name);
}
