using AppSimple.Core.Interfaces;
using AppSimple.Core.Logging;
using AppSimple.Core.Models;

namespace AppSimple.Core.Services.Impl;

/// <summary>
/// Core implementation of <see cref="INoteService"/> backed by <see cref="INoteRepository"/>.
/// </summary>
public sealed class NoteService : INoteService
{
    private readonly INoteRepository _notes;
    private readonly IAppLogger<NoteService> _logger;

    /// <summary>Initializes a new instance of <see cref="NoteService"/>.</summary>
    public NoteService(INoteRepository notes, IAppLogger<NoteService> logger)
    {
        _notes  = notes;
        _logger = logger;
        _logger.Debug("NoteService initialized.");
    }

    /// <inheritdoc />
    public Task<Note?> GetByUidAsync(Guid uid)
    {
        _logger.Debug("GetByUid: {Uid}", uid);
        return _notes.GetByUidAsync(uid);
    }

    /// <inheritdoc />
    public Task<IEnumerable<Note>> GetAllAsync()
    {
        _logger.Debug("GetAll notes requested.");
        return _notes.GetAllAsync();
    }

    /// <inheritdoc />
    public Task<IEnumerable<Note>> GetByUserUidAsync(Guid userUid)
    {
        _logger.Debug("GetByUserUid: {UserUid}", userUid);
        return _notes.GetByUserUidAsync(userUid);
    }

    /// <inheritdoc />
    public async Task<Note> CreateAsync(Guid userUid, string title, string content)
    {
        var now  = DateTime.UtcNow;
        var note = new Note
        {
            Uid       = Guid.CreateVersion7(),
            UserUid   = userUid,
            Title     = title,
            Content   = content,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _notes.AddAsync(note);
        _logger.Information("Note {Uid} created for user {UserUid}.", note.Uid, userUid);
        return note;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Note note)
    {
        note.UpdatedAt = DateTime.UtcNow;
        await _notes.UpdateAsync(note);
        _logger.Information("Note {Uid} updated.", note.Uid);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid uid)
    {
        await _notes.DeleteAsync(uid);
        _logger.Information("Note {Uid} deleted.", uid);
    }

    /// <inheritdoc />
    public async Task AddTagAsync(Guid noteUid, Guid tagUid)
    {
        await _notes.AddTagAsync(noteUid, tagUid);
        _logger.Debug("Tag {TagUid} added to note {NoteUid}.", tagUid, noteUid);
    }

    /// <inheritdoc />
    public async Task RemoveTagAsync(Guid noteUid, Guid tagUid)
    {
        await _notes.RemoveTagAsync(noteUid, tagUid);
        _logger.Debug("Tag {TagUid} removed from note {NoteUid}.", tagUid, noteUid);
    }
}
