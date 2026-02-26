namespace AppSimple.Core.Models.Requests;

/// <summary>Request payload for creating a new tag.</summary>
public sealed class CreateTagRequest
{
    /// <summary>Gets or sets the display name of the tag.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets an optional description of the tag.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the hex color for the tag (e.g. <c>#FF0000</c>). Defaults to <c>#CCCCCC</c>.</summary>
    public string? Color { get; set; } = "#CCCCCC";
}
