using AppSimple.Core.Models;

namespace AppSimple.Core.Services;

/// <summary>
/// Service for managing <see cref="Note"/> entities.
/// </summary>
/// <remarks>
/// Access rules:
/// <list type="bullet">
///   <item>Users may only read and modify their own notes.</item>
///   <item>Admins may read all notes, but may only edit or delete notes they own.</item>
/// </list>
/// </remarks>
public interface INoteService
{
    /// <summary>Gets a note by its unique identifier.</summary>
    /// <param name="uid">The UID of the note.</param>
    /// <returns>The note, or <c>null</c> if not found.</returns>
    Task<Note?> GetByUidAsync(Guid uid);

    /// <summary>Returns all notes in the system. Intended for admin use.</summary>
    Task<IEnumerable<Note>> GetAllAsync();

    /// <summary>Returns all notes belonging to the specified user.</summary>
    /// <param name="userUid">The UID of the user.</param>
    Task<IEnumerable<Note>> GetByUserUidAsync(Guid userUid);

    /// <summary>Creates a new note owned by the specified user.</summary>
    /// <param name="userUid">The UID of the note owner.</param>
    /// <param name="title">Optional title for the note.</param>
    /// <param name="content">The body content of the note.</param>
    /// <returns>The newly created <see cref="Note"/>.</returns>
    Task<Note> CreateAsync(Guid userUid, string title, string content);

    /// <summary>Updates the title and/or content of an existing note.</summary>
    /// <param name="note">The note entity with updated values.</param>
    Task UpdateAsync(Note note);

    /// <summary>Deletes a note by its unique identifier.</summary>
    /// <param name="uid">The UID of the note to delete.</param>
    Task DeleteAsync(Guid uid);

    /// <summary>Associates a tag with a note.</summary>
    /// <param name="noteUid">The UID of the note.</param>
    /// <param name="tagUid">The UID of the tag to add.</param>
    Task AddTagAsync(Guid noteUid, Guid tagUid);

    /// <summary>Removes a tag association from a note.</summary>
    /// <param name="noteUid">The UID of the note.</param>
    /// <param name="tagUid">The UID of the tag to remove.</param>
    Task RemoveTagAsync(Guid noteUid, Guid tagUid);
}
