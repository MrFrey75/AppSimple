namespace AppSimple.Core.Models.Requests;

/// <summary>Request payload for updating an existing note.</summary>
public sealed class UpdateNoteRequest
{
    /// <summary>Gets or sets the updated title. <c>null</c> means no change.</summary>
    public string? Title { get; set; }

    /// <summary>Gets or sets the updated content. <c>null</c> means no change.</summary>
    public string? Content { get; set; }
}
