using AppSimple.Core.Models;

namespace AppSimple.Core.Services;

/// <summary>
/// Service for managing <see cref="Tag"/> entities.
/// </summary>
/// <remarks>
/// Tags are user-specific. Admins may read all tags, but may only edit or delete their own.
/// </remarks>
public interface ITagService
{
    /// <summary>Gets a tag by its unique identifier.</summary>
    /// <param name="uid">The UID of the tag.</param>
    /// <returns>The tag, or <c>null</c> if not found.</returns>
    Task<Tag?> GetByUidAsync(Guid uid);

    /// <summary>Returns all tags in the system. Intended for admin use.</summary>
    Task<IEnumerable<Tag>> GetAllAsync();

    /// <summary>Returns all tags belonging to the specified user.</summary>
    /// <param name="userUid">The UID of the user.</param>
    Task<IEnumerable<Tag>> GetByUserUidAsync(Guid userUid);

    /// <summary>Creates a new tag owned by the specified user.</summary>
    /// <param name="userUid">The UID of the tag owner.</param>
    /// <param name="name">The display name of the tag.</param>
    /// <param name="description">Optional description.</param>
    /// <param name="color">Optional hex color string (e.g. <c>#FF0000</c>).</param>
    /// <returns>The newly created <see cref="Tag"/>.</returns>
    Task<Tag> CreateAsync(Guid userUid, string name, string? description = null, string? color = null);

    /// <summary>Updates an existing tag's properties.</summary>
    /// <param name="tag">The tag entity with updated values.</param>
    Task UpdateAsync(Tag tag);

    /// <summary>Deletes a tag by its unique identifier. Also removes all note associations.</summary>
    /// <param name="uid">The UID of the tag to delete.</param>
    Task DeleteAsync(Guid uid);
}
