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
    }

    /// <inheritdoc />
    public async Task<Note?> GetByUidAsync(Guid uid)
    {
        try
        {
            return await _notes.GetByUidAsync(uid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving note {Uid}.", uid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Note>> GetAllAsync()
    {
        try
        {
            return await _notes.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving all notes.");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Note>> GetByUserUidAsync(Guid userUid)
    {
        try
        {
            return await _notes.GetByUserUidAsync(userUid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving notes for user {UserUid}.", userUid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Note> CreateAsync(Guid userUid, string title, string content)
    {
        try
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
        catch (Exception ex)
        {
            _logger.Error(ex, "Error creating note for user {UserUid}.", userUid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Note note)
    {
        try
        {
            note.UpdatedAt = DateTime.UtcNow;
            await _notes.UpdateAsync(note);
            _logger.Information("Note {Uid} updated.", note.Uid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error updating note {Uid}.", note.Uid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid uid)
    {
        try
        {
            await _notes.DeleteAsync(uid);
            _logger.Information("Note {Uid} deleted.", uid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error deleting note {Uid}.", uid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task AddTagAsync(Guid noteUid, Guid tagUid)
    {
        try
        {
            await _notes.AddTagAsync(noteUid, tagUid);
            _logger.Debug("Tag {TagUid} added to note {NoteUid}.", tagUid, noteUid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error adding tag {TagUid} to note {NoteUid}.", tagUid, noteUid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task RemoveTagAsync(Guid noteUid, Guid tagUid)
    {
        try
        {
            await _notes.RemoveTagAsync(noteUid, tagUid);
            _logger.Debug("Tag {TagUid} removed from note {NoteUid}.", tagUid, noteUid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error removing tag {TagUid} from note {NoteUid}.", tagUid, noteUid);
            throw;
        }
    }
}
