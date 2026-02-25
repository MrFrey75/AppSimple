using AppSimple.Core.Interfaces;
using AppSimple.Core.Models;
using AppSimple.DataLib.Db;
using Dapper;
using Serilog;

namespace AppSimple.DataLib.Repositories;

/// <summary>
/// SQLite/Dapper implementation of <see cref="IUserRepository"/>.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger _logger = Log.ForContext<UserRepository>();

    /// <summary>
    /// Initializes a new instance of <see cref="UserRepository"/>.
    /// </summary>
    /// <param name="connectionFactory">The factory used to create database connections.</param>
    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc />
    public async Task<User?> GetByUidAsync(Guid uid)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Uid = @Uid",
            new { Uid = uid.ToString() });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<User>("SELECT * FROM Users ORDER BY Username");
    }

    /// <inheritdoc />
    public async Task AddAsync(User entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO Users (Uid, Username, PasswordHash, Email, FirstName, LastName, PhoneNumber, DateOfBirth, Bio, AvatarUrl, Role, IsActive, IsSystem, CreatedAt, UpdatedAt)
            VALUES (@Uid, @Username, @PasswordHash, @Email, @FirstName, @LastName, @PhoneNumber, @DateOfBirth, @Bio, @AvatarUrl, @Role, @IsActive, @IsSystem, @CreatedAt, @UpdatedAt)
            """, MapToParams(entity));
        _logger.Information("User {Username} created.", entity.Username);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(User entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("""
            UPDATE Users SET
                Username     = @Username,
                PasswordHash = @PasswordHash,
                Email        = @Email,
                FirstName    = @FirstName,
                LastName     = @LastName,
                PhoneNumber  = @PhoneNumber,
                DateOfBirth  = @DateOfBirth,
                Bio          = @Bio,
                AvatarUrl    = @AvatarUrl,
                Role         = @Role,
                IsActive     = @IsActive,
                UpdatedAt    = @UpdatedAt
            WHERE Uid = @Uid AND IsSystem = 0
            """, MapToParams(entity));
        _logger.Information("User {Uid} updated.", entity.Uid);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid uid)
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync(
            "DELETE FROM Users WHERE Uid = @Uid AND IsSystem = 0",
            new { Uid = uid.ToString() });

        if (rows == 0)
            _logger.Warning("Delete skipped for Uid {Uid} (not found or system entity).", uid);
        else
            _logger.Information("User {Uid} deleted.", uid);
    }

    /// <inheritdoc />
    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Username = @Username COLLATE NOCASE",
            new { Username = username });
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Email = @Email COLLATE NOCASE",
            new { Email = email });
    }

    /// <inheritdoc />
    public async Task<bool> UsernameExistsAsync(string username)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Users WHERE Username = @Username COLLATE NOCASE",
            new { Username = username }) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> EmailExistsAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Users WHERE Email = @Email COLLATE NOCASE",
            new { Email = email }) > 0;
    }

    private static object MapToParams(User u) => new
    {
        Uid = u.Uid.ToString(),
        u.Username,
        u.PasswordHash,
        u.Email,
        u.FirstName,
        u.LastName,
        u.PhoneNumber,
        DateOfBirth = u.DateOfBirth?.ToString("O"),
        u.Bio,
        u.AvatarUrl,
        Role = (int)u.Role,
        IsActive = u.IsActive ? 1 : 0,
        IsSystem = u.IsSystem ? 1 : 0,
        CreatedAt = u.CreatedAt.ToString("O"),
        UpdatedAt = u.UpdatedAt.ToString("O")
    };
}
