using AppSimple.Core.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace AppSimple.WebApp.Models;

/// <summary>ViewModel for the notes list page.</summary>
public sealed class NoteListViewModel
{
    /// <summary>Gets or sets the list of notes.</summary>
    public IReadOnlyList<NoteDto> Notes { get; set; } = [];

    /// <summary>Gets or sets an error message, if any.</summary>
    public string? Error { get; set; }
}

/// <summary>ViewModel for the note detail page.</summary>
public sealed class NoteDetailViewModel
{
    /// <summary>Gets or sets the note being viewed.</summary>
    public required NoteDto Note { get; set; }

    /// <summary>Gets or sets all tags owned by the current user (for add-tag dropdown).</summary>
    public IReadOnlyList<TagDto> AllTags { get; set; } = [];
}

/// <summary>ViewModel for creating a new note.</summary>
public sealed class CreateNoteViewModel
{
    /// <summary>Gets or sets the note title (optional).</summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>Gets or sets the note content.</summary>
    [Required(ErrorMessage = "Content is required.")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>ViewModel for editing an existing note.</summary>
public sealed class EditNoteViewModel
{
    /// <summary>Gets or sets the note UID.</summary>
    public Guid Uid { get; set; }

    /// <summary>Gets or sets the note title.</summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>Gets or sets the note content.</summary>
    [Required(ErrorMessage = "Content is required.")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>ViewModel for the tags list page.</summary>
public sealed class TagListViewModel
{
    /// <summary>Gets or sets the list of tags.</summary>
    public IReadOnlyList<TagDto> Tags { get; set; } = [];

    /// <summary>Gets or sets an error message, if any.</summary>
    public string? Error { get; set; }
}

/// <summary>ViewModel for creating a new tag.</summary>
public sealed class CreateTagViewModel
{
    /// <summary>Gets or sets the tag name.</summary>
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the tag description.</summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>Gets or sets the hex color for the tag.</summary>
    public string Color { get; set; } = "#CCCCCC";
}

/// <summary>ViewModel for editing an existing tag.</summary>
public sealed class EditTagViewModel
{
    /// <summary>Gets or sets the tag UID.</summary>
    public Guid Uid { get; set; }

    /// <summary>Gets or sets the tag name.</summary>
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the tag description.</summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>Gets or sets the hex color for the tag.</summary>
    public string Color { get; set; } = "#CCCCCC";
}
