namespace AppSimple.Core.Models;

/// <summary>
/// Represents a user note with an optional title, rich content, and associated tags.
/// </summary>
public class Note : BaseEntity
{
    /// <summary>Gets or sets the title of the note.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the content/body of the note.</summary>
    public required string Content { get; set; }

    /// <summary>Gets or sets the UID of the user who owns this note.</summary>
    public required Guid UserUid { get; set; }

    /// <summary>Gets or sets the tags associated with this note. Populated by the repository layer.</summary>
    public List<Tag> Tags { get; set; } = [];
}
