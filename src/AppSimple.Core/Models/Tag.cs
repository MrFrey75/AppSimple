namespace AppSimple.Core.Models;

/// <summary>
/// Represents a user-defined label that can be attached to one or more notes.
/// </summary>
public class Tag : BaseEntity
{
    /// <summary>Gets or sets the display name of the tag.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets an optional description of the tag.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the UID of the user who created this tag.</summary>
    public required Guid UserUid { get; set; }

    /// <summary>
    /// Gets or sets the hex color associated with this tag (e.g. <c>#FF0000</c> for red).
    /// Defaults to light gray (<c>#CCCCCC</c>).
    /// </summary>
    public string? Color { get; set; } = "#CCCCCC";
}
