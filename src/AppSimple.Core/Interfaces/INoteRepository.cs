using AppSimple.Core.Models;

namespace AppSimple.Core.Interfaces;

/// <summary>
/// Repository interface for <see cref="Note"/> entities.
/// Extends the generic repository with note-specific queries and tag-association operations.
/// </summary>
public interface INoteRepository : IRepository<Note>
{
    /// <summary>Returns all notes belonging to a specific user, with tags populated.</summary>
    /// <param name="userUid">The UID of the owning user.</param>
    Task<IEnumerable<Note>> GetByUserUidAsync(Guid userUid);

    /// <summary>Associates a tag with a note.</summary>
    /// <param name="noteUid">The UID of the note.</param>
    /// <param name="tagUid">The UID of the tag.</param>
    Task AddTagAsync(Guid noteUid, Guid tagUid);

    /// <summary>Removes a tag association from a note.</summary>
    /// <param name="noteUid">The UID of the note.</param>
    /// <param name="tagUid">The UID of the tag.</param>
    Task RemoveTagAsync(Guid noteUid, Guid tagUid);
}
