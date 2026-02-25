using AppSimple.Core.Auth;
using AppSimple.Core.Constants;
using AppSimple.Core.Logging;
using AppSimple.Core.Services;
using AppSimple.DataLib.Db;
using Dapper;

namespace AppSimple.DataLib.Services.Impl;

/// <summary>
/// Implements <see cref="IDatabaseResetService"/> using direct SQLite access.
/// Drops all user data, recreates the schema, and reseeds default users.
/// </summary>
public sealed class DatabaseResetService : IDatabaseResetService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly DbInitializer        _initializer;
    private readonly IPasswordHasher      _hasher;
    private readonly IAppLogger<DatabaseResetService> _logger;

    /// <summary>Initializes a new instance of <see cref="DatabaseResetService"/>.</summary>
    public DatabaseResetService(
        IDbConnectionFactory connectionFactory,
        DbInitializer        initializer,
        IPasswordHasher      hasher,
        IAppLogger<DatabaseResetService> logger)
    {
        _connectionFactory = connectionFactory;
        _initializer       = initializer;
        _hasher            = hasher;
        _logger            = logger;
    }

    /// <inheritdoc />
    public Task ResetAndReseedAsync()
    {
        _logger.Warning("Database reset initiated — all data will be erased.");

        using var connection = _connectionFactory.CreateConnection();

        // 1 — Wipe all tables
        connection.Execute("DELETE FROM Users;");
        _logger.Information("All rows deleted from Users table.");

        // 2 — Recreate schema (idempotent — IF NOT EXISTS guards are in place)
        _initializer.Initialize();

        // 3 — Seed default admin user
        var adminHash = _hasher.Hash(AppConstants.DefaultAdminPassword);
        _initializer.SeedAdminUser(adminHash);
        _logger.Information("Default admin user seeded (username: '{Username}').", AppConstants.DefaultAdminUsername);

        // 4 — Seed sample users
        SeedSampleUsers(connection);

        _logger.Information("Database reset and reseed complete.");
        return Task.CompletedTask;
    }

    private void SeedSampleUsers(System.Data.IDbConnection connection)
    {
        var sampleUsers = new[]
        {
            ("alice",  "alice@appsimple.dev",  "Alice",  "Johnson"),
            ("bob",    "bob@appsimple.dev",    "Bob",    "Smith"),
            ("carol",  "carol@appsimple.dev",  "Carol",  "Williams"),
        };

        var samplePassword = _hasher.Hash(AppConstants.DefaultSamplePassword);
        var now            = DateTime.UtcNow.ToString("O");

        foreach (var (username, email, firstName, lastName) in sampleUsers)
        {
            var uid = Guid.CreateVersion7().ToString();
            connection.Execute("""
                INSERT OR IGNORE INTO Users
                    (Uid, Username, PasswordHash, Email, FirstName, LastName,
                     Role, IsActive, IsSystem, CreatedAt, UpdatedAt)
                VALUES
                    (@Uid, @Username, @PasswordHash, @Email, @FirstName, @LastName,
                     0, 1, 0, @Now, @Now)
                """,
                new { Uid = uid, Username = username, PasswordHash = samplePassword,
                      Email = email, FirstName = firstName, LastName = lastName, Now = now });

            _logger.Information("Seeded sample user '{Username}'.", username);
        }
    }
}
