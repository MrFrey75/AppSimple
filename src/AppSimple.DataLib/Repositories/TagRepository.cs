using AppSimple.Core.Interfaces;
using AppSimple.Core.Models;
using AppSimple.DataLib.Db;
using Dapper;
using Serilog;

namespace AppSimple.DataLib.Repositories;

/// <summary>
/// SQLite/Dapper implementation of <see cref="ITagRepository"/>.
/// </summary>
public sealed class TagRepository : ITagRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger _logger = Log.ForContext<TagRepository>();

    /// <summary>Initializes a new instance of <see cref="TagRepository"/>.</summary>
    /// <param name="connectionFactory">The factory used to create database connections.</param>
    public TagRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc />
    public async Task<Tag?> GetByUidAsync(Guid uid)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Tag>(
            "SELECT * FROM Tags WHERE Uid = @Uid",
            new { Uid = uid.ToString() });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Tag>("SELECT * FROM Tags ORDER BY Name");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Tag>> GetByUserUidAsync(Guid userUid)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Tag>(
            "SELECT * FROM Tags WHERE UserUid = @UserUid ORDER BY Name",
            new { UserUid = userUid.ToString() });
    }

    /// <inheritdoc />
    public async Task<Tag?> GetByNameAsync(Guid userUid, string name)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Tag>(
            "SELECT * FROM Tags WHERE UserUid = @UserUid AND Name = @Name COLLATE NOCASE",
            new { UserUid = userUid.ToString(), Name = name });
    }

    /// <inheritdoc />
    public async Task AddAsync(Tag entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO Tags (Uid, UserUid, Name, Description, Color, IsSystem, CreatedAt, UpdatedAt)
            VALUES (@Uid, @UserUid, @Name, @Description, @Color, @IsSystem, @CreatedAt, @UpdatedAt)
            """, MapToParams(entity));
        _logger.Information("Tag '{Name}' ({Uid}) created.", entity.Name, entity.Uid);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Tag entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("""
            UPDATE Tags SET
                Name        = @Name,
                Description = @Description,
                Color       = @Color,
                UpdatedAt   = @UpdatedAt
            WHERE Uid = @Uid
            """, MapToParams(entity));
        _logger.Information("Tag {Uid} updated.", entity.Uid);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid uid)
    {
        using var connection = _connectionFactory.CreateConnection();
        // NoteTags rows are removed by ON DELETE CASCADE
        await connection.ExecuteAsync(
            "DELETE FROM Tags WHERE Uid = @Uid",
            new { Uid = uid.ToString() });
        _logger.Information("Tag {Uid} deleted.", uid);
    }

    private static object MapToParams(Tag t) => new
    {
        Uid         = t.Uid.ToString(),
        UserUid     = t.UserUid.ToString(),
        t.Name,
        t.Description,
        t.Color,
        IsSystem    = t.IsSystem ? 1 : 0,
        CreatedAt   = t.CreatedAt.ToString("O"),
        UpdatedAt   = t.UpdatedAt.ToString("O"),
    };
}
