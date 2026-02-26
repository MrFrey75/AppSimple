using AppSimple.Core.Models;

namespace AppSimple.Core.Models.DTOs;

/// <summary>
/// Read-only projection of a <see cref="Tag"/> for API and UI consumption.
/// </summary>
public sealed class TagDto
{
    /// <summary>Gets the tag's unique identifier.</summary>
    public Guid Uid { get; init; }

    /// <summary>Gets the display name of the tag.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the optional description of the tag.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the UID of the user who created the tag.</summary>
    public Guid UserUid { get; init; }

    /// <summary>Gets the hex color of the tag.</summary>
    public string? Color { get; init; }

    /// <summary>Gets the UTC timestamp when the tag was created.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Creates a <see cref="TagDto"/> from a <see cref="Tag"/> entity.</summary>
    /// <param name="tag">The source entity.</param>
    /// <returns>A populated <see cref="TagDto"/>.</returns>
    public static TagDto From(Tag tag) => new()
    {
        Uid         = tag.Uid,
        Name        = tag.Name,
        Description = tag.Description,
        UserUid     = tag.UserUid,
        Color       = tag.Color,
        CreatedAt   = tag.CreatedAt,
    };
}
