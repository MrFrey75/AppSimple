using AppSimple.Core.Logging;
using AppSimple.Core.Models.DTOs;
using AppSimple.Core.Models.Requests;
using AppSimple.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppSimple.WebApi.Controllers;

/// <summary>REST endpoints for note and tag management.</summary>
[ApiController]
[Authorize]
public sealed class NotesController : ControllerBase
{
    private readonly INoteService _notes;
    private readonly ITagService _tags;
    private readonly IUserService _users;
    private readonly IAppLogger<NotesController> _logger;

    /// <summary>Initializes a new instance of <see cref="NotesController"/>.</summary>
    public NotesController(INoteService notes, ITagService tags, IUserService users, IAppLogger<NotesController> logger)
    {
        _notes  = notes;
        _tags   = tags;
        _users  = users;
        _logger = logger;
    }

    private async Task<Guid?> GetUserUidAsync()
    {
        var username = User.Identity?.Name;
        if (username is null) return null;
        var user = await _users.GetByUsernameAsync(username);
        return user?.Uid;
    }

    // ── Notes ──────────────────────────────────────────────────────────────

    /// <summary>Returns all notes belonging to the authenticated user.</summary>
    [HttpGet("api/notes")]
    [ProducesResponseType(typeof(IEnumerable<NoteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotes()
    {
        var userUid = await GetUserUidAsync();
        if (userUid is null) return Unauthorized();

        var notes = await _notes.GetByUserUidAsync(userUid.Value);
        return Ok(notes.Select(NoteDto.From));
    }

    /// <summary>Returns a single note by UID.</summary>
    [HttpGet("api/notes/{uid:guid}")]
    [ProducesResponseType(typeof(NoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNote(Guid uid)
    {
        var note = await _notes.GetByUidAsync(uid);
        if (note is null) return NotFound();
        return Ok(NoteDto.From(note));
    }

    /// <summary>Creates a new note for the authenticated user.</summary>
    [HttpPost("api/notes")]
    [ProducesResponseType(typeof(NoteDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateNote([FromBody] CreateNoteRequest request)
    {
        var userUid = await GetUserUidAsync();
        if (userUid is null) return Unauthorized();

        var note = await _notes.CreateAsync(userUid.Value, request.Title, request.Content);
        _logger.Information("User {User} created note {Uid}", User.Identity?.Name, note.Uid);
        return CreatedAtAction(nameof(GetNote), new { uid = note.Uid }, NoteDto.From(note));
    }

    /// <summary>Updates an existing note.</summary>
    [HttpPut("api/notes/{uid:guid}")]
    [ProducesResponseType(typeof(NoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateNote(Guid uid, [FromBody] UpdateNoteRequest request)
    {
        var note = await _notes.GetByUidAsync(uid);
        if (note is null) return NotFound();

        if (request.Title   is not null) note.Title   = request.Title;
        if (request.Content is not null) note.Content = request.Content;

        await _notes.UpdateAsync(note);
        var updated = await _notes.GetByUidAsync(uid);
        return Ok(NoteDto.From(updated!));
    }

    /// <summary>Deletes a note by UID.</summary>
    [HttpDelete("api/notes/{uid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNote(Guid uid)
    {
        var note = await _notes.GetByUidAsync(uid);
        if (note is null) return NotFound();

        await _notes.DeleteAsync(uid);
        _logger.Information("User {User} deleted note {Uid}", User.Identity?.Name, uid);
        return NoContent();
    }

    /// <summary>Adds a tag to a note.</summary>
    [HttpPost("api/notes/{noteUid:guid}/tags/{tagUid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTag(Guid noteUid, Guid tagUid)
    {
        var note = await _notes.GetByUidAsync(noteUid);
        if (note is null) return NotFound();

        await _notes.AddTagAsync(noteUid, tagUid);
        return NoContent();
    }

    /// <summary>Removes a tag from a note.</summary>
    [HttpDelete("api/notes/{noteUid:guid}/tags/{tagUid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveTag(Guid noteUid, Guid tagUid)
    {
        await _notes.RemoveTagAsync(noteUid, tagUid);
        return NoContent();
    }

    // ── Tags ───────────────────────────────────────────────────────────────

    /// <summary>Returns all tags belonging to the authenticated user.</summary>
    [HttpGet("api/tags")]
    [ProducesResponseType(typeof(IEnumerable<TagDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTags()
    {
        var userUid = await GetUserUidAsync();
        if (userUid is null) return Unauthorized();

        var tags = await _tags.GetByUserUidAsync(userUid.Value);
        return Ok(tags.Select(TagDto.From));
    }

    /// <summary>Returns a single tag by UID.</summary>
    [HttpGet("api/tags/{uid:guid}")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTag(Guid uid)
    {
        var tag = await _tags.GetByUidAsync(uid);
        if (tag is null) return NotFound();
        return Ok(TagDto.From(tag));
    }

    /// <summary>Creates a new tag for the authenticated user.</summary>
    [HttpPost("api/tags")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request)
    {
        var userUid = await GetUserUidAsync();
        if (userUid is null) return Unauthorized();

        var tag = await _tags.CreateAsync(userUid.Value, request.Name, request.Description, request.Color);
        return CreatedAtAction(nameof(GetTag), new { uid = tag.Uid }, TagDto.From(tag));
    }

    /// <summary>Updates an existing tag.</summary>
    [HttpPut("api/tags/{uid:guid}")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTag(Guid uid, [FromBody] UpdateTagRequest request)
    {
        var tag = await _tags.GetByUidAsync(uid);
        if (tag is null) return NotFound();

        if (request.Name        is not null) tag.Name        = request.Name;
        if (request.Description is not null) tag.Description = request.Description;
        if (request.Color       is not null) tag.Color       = request.Color;

        await _tags.UpdateAsync(tag);
        var updated = await _tags.GetByUidAsync(uid);
        return Ok(TagDto.From(updated!));
    }

    /// <summary>Deletes a tag by UID.</summary>
    [HttpDelete("api/tags/{uid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTag(Guid uid)
    {
        var tag = await _tags.GetByUidAsync(uid);
        if (tag is null) return NotFound();

        await _tags.DeleteAsync(uid);
        return NoContent();
    }
}
