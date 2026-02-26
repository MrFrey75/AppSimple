namespace AppSimple.Core.Models.Requests;

/// <summary>Request payload for updating an existing tag.</summary>
public sealed class UpdateTagRequest
{
    /// <summary>Gets or sets the updated name. <c>null</c> means no change.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the updated description. <c>null</c> means no change.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the updated hex color. <c>null</c> means no change.</summary>
    public string? Color { get; set; }
}
