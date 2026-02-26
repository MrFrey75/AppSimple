using AppSimple.Core.Interfaces;
using AppSimple.Core.Models;
using AppSimple.DataLib.Db;
using Dapper;
using Serilog;

namespace AppSimple.DataLib.Repositories;

/// <summary>
/// SQLite/Dapper implementation of <see cref="INoteRepository"/>.
/// Tags are loaded via a JOIN on the <c>NoteTags</c> junction table.
/// </summary>
public sealed class NoteRepository : INoteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger _logger = Log.ForContext<NoteRepository>();

    /// <summary>Initializes a new instance of <see cref="NoteRepository"/>.</summary>
    /// <param name="connectionFactory">The factory used to create database connections.</param>
    public NoteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc />
    public async Task<Note?> GetByUidAsync(Guid uid)
    {
        using var connection = _connectionFactory.CreateConnection();
        var note = await connection.QuerySingleOrDefaultAsync<Note>(
            "SELECT * FROM Notes WHERE Uid = @Uid",
            new { Uid = uid.ToString() });

        if (note is not null)
            note.Tags = (await LoadTagsAsync(connection, uid)).ToList();

        return note;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Note>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var notes = (await connection.QueryAsync<Note>("SELECT * FROM Notes ORDER BY UpdatedAt DESC")).ToList();
        foreach (var note in notes)
            note.Tags = (await LoadTagsAsync(connection, note.Uid)).ToList();
        return notes;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Note>> GetByUserUidAsync(Guid userUid)
    {
        using var connection = _connectionFactory.CreateConnection();
        var notes = (await connection.QueryAsync<Note>(
            "SELECT * FROM Notes WHERE UserUid = @UserUid ORDER BY UpdatedAt DESC",
            new { UserUid = userUid.ToString() })).ToList();

        foreach (var note in notes)
            note.Tags = (await LoadTagsAsync(connection, note.Uid)).ToList();

        return notes;
    }

    /// <inheritdoc />
    public async Task AddAsync(Note entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO Notes (Uid, UserUid, Title, Content, IsSystem, CreatedAt, UpdatedAt)
            VALUES (@Uid, @UserUid, @Title, @Content, @IsSystem, @CreatedAt, @UpdatedAt)
            """, MapToParams(entity));
        _logger.Information("Note {Uid} created.", entity.Uid);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Note entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("""
            UPDATE Notes SET
                Title     = @Title,
                Content   = @Content,
                UpdatedAt = @UpdatedAt
            WHERE Uid = @Uid
            """, MapToParams(entity));
        _logger.Information("Note {Uid} updated.", entity.Uid);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid uid)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "DELETE FROM Notes WHERE Uid = @Uid",
            new { Uid = uid.ToString() });
        _logger.Information("Note {Uid} deleted.", uid);
    }

    /// <inheritdoc />
    public async Task AddTagAsync(Guid noteUid, Guid tagUid)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT OR IGNORE INTO NoteTags (NoteUid, TagUid) VALUES (@NoteUid, @TagUid)
            """, new { NoteUid = noteUid.ToString(), TagUid = tagUid.ToString() });
    }

    /// <inheritdoc />
    public async Task RemoveTagAsync(Guid noteUid, Guid tagUid)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "DELETE FROM NoteTags WHERE NoteUid = @NoteUid AND TagUid = @TagUid",
            new { NoteUid = noteUid.ToString(), TagUid = tagUid.ToString() });
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static async Task<IEnumerable<Tag>> LoadTagsAsync(
        System.Data.IDbConnection connection, Guid noteUid)
    {
        return await connection.QueryAsync<Tag>("""
            SELECT t.* FROM Tags t
            INNER JOIN NoteTags nt ON t.Uid = nt.TagUid
            WHERE nt.NoteUid = @NoteUid
            """, new { NoteUid = noteUid.ToString() });
    }

    private static object MapToParams(Note n) => new
    {
        Uid       = n.Uid.ToString(),
        UserUid   = n.UserUid.ToString(),
        n.Title,
        n.Content,
        IsSystem  = n.IsSystem ? 1 : 0,
        CreatedAt = n.CreatedAt.ToString("O"),
        UpdatedAt = n.UpdatedAt.ToString("O"),
    };
}
