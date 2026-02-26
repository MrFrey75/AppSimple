namespace AppSimple.Core.Models.Requests;

/// <summary>Request payload for creating a new note.</summary>
public sealed class CreateNoteRequest
{
    /// <summary>Gets or sets the title of the note. Optional.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the content/body of the note.</summary>
    public required string Content { get; set; }

    /// <summary>Gets or sets the UIDs of tags to associate with the note on creation.</summary>
    public List<Guid> TagUids { get; set; } = [];
}
