using AppSimple.Core.Models;

namespace AppSimple.Core.Models.DTOs;

/// <summary>
/// Read-only projection of a <see cref="Note"/> for API and UI consumption.
/// </summary>
public sealed class NoteDto
{
    /// <summary>Gets the note's unique identifier.</summary>
    public Guid Uid { get; init; }

    /// <summary>Gets the title of the note.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the content/body of the note.</summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>Gets the UID of the user who owns the note.</summary>
    public Guid UserUid { get; init; }

    /// <summary>Gets the tags associated with the note.</summary>
    public IReadOnlyList<TagDto> Tags { get; init; } = [];

    /// <summary>Gets the UTC timestamp when the note was created.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets the UTC timestamp when the note was last updated.</summary>
    public DateTime UpdatedAt { get; init; }

    /// <summary>Creates a <see cref="NoteDto"/> from a <see cref="Note"/> entity.</summary>
    /// <param name="note">The source entity.</param>
    /// <returns>A populated <see cref="NoteDto"/>.</returns>
    public static NoteDto From(Note note) => new()
    {
        Uid       = note.Uid,
        Title     = note.Title,
        Content   = note.Content,
        UserUid   = note.UserUid,
        Tags      = note.Tags.Select(TagDto.From).ToList(),
        CreatedAt = note.CreatedAt,
        UpdatedAt = note.UpdatedAt,
    };
}
