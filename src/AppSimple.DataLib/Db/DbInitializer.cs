using AppSimple.Core.Constants;
using AppSimple.Core.Enums;
using AppSimple.Core.Models;
using Dapper;
using Serilog;

namespace AppSimple.DataLib.Db;

/// <summary>
/// Handles SQLite schema creation and optional seeding of system data.
/// </summary>
public sealed class DbInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger _logger = Log.ForContext<DbInitializer>();

    /// <summary>
    /// Initializes a new instance of <see cref="DbInitializer"/>.
    /// </summary>
    /// <param name="connectionFactory">The factory used to open database connections.</param>
    public DbInitializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Creates the database schema if it does not already exist.
    /// </summary>
    public void Initialize()
    {
        _logger.Information("Initializing database schema...");

        using var connection = _connectionFactory.CreateConnection();

        connection.Execute("""
            CREATE TABLE IF NOT EXISTS Users (
                Uid          TEXT NOT NULL PRIMARY KEY,
                Username     TEXT NOT NULL UNIQUE COLLATE NOCASE,
                PasswordHash TEXT NOT NULL,
                Email        TEXT NOT NULL UNIQUE COLLATE NOCASE,
                FirstName    TEXT,
                LastName     TEXT,
                PhoneNumber  TEXT,
                DateOfBirth  TEXT,
                Bio          TEXT,
                AvatarUrl    TEXT,
                Role         INTEGER NOT NULL DEFAULT 0,
                IsActive     INTEGER NOT NULL DEFAULT 1,
                IsSystem     INTEGER NOT NULL DEFAULT 0,
                CreatedAt    TEXT NOT NULL,
                UpdatedAt    TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Tags (
                Uid         TEXT NOT NULL PRIMARY KEY,
                UserUid     TEXT NOT NULL REFERENCES Users(Uid) ON DELETE CASCADE,
                Name        TEXT NOT NULL COLLATE NOCASE,
                Description TEXT,
                Color       TEXT NOT NULL DEFAULT '#CCCCCC',
                IsSystem    INTEGER NOT NULL DEFAULT 0,
                CreatedAt   TEXT NOT NULL,
                UpdatedAt   TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Notes (
                Uid       TEXT NOT NULL PRIMARY KEY,
                UserUid   TEXT NOT NULL REFERENCES Users(Uid) ON DELETE CASCADE,
                Title     TEXT NOT NULL DEFAULT '',
                Content   TEXT NOT NULL,
                IsSystem  INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS NoteTags (
                NoteUid TEXT NOT NULL REFERENCES Notes(Uid) ON DELETE CASCADE,
                TagUid  TEXT NOT NULL REFERENCES Tags(Uid)  ON DELETE CASCADE,
                PRIMARY KEY (NoteUid, TagUid)
            );

            CREATE TABLE IF NOT EXISTS Contacts (
                Uid          TEXT NOT NULL PRIMARY KEY,
                OwnerUserUid TEXT NOT NULL REFERENCES Users(Uid) ON DELETE CASCADE,
                Name         TEXT NOT NULL COLLATE NOCASE,
                Tags         TEXT NOT NULL DEFAULT '[]',
                IsSystem     INTEGER NOT NULL DEFAULT 0,
                CreatedAt    TEXT NOT NULL,
                UpdatedAt    TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ContactEmailAddresses (
                Uid        TEXT NOT NULL PRIMARY KEY,
                ContactUid TEXT NOT NULL REFERENCES Contacts(Uid) ON DELETE CASCADE,
                Email      TEXT NOT NULL COLLATE NOCASE,
                IsPrimary  INTEGER NOT NULL DEFAULT 0,
                Tags       TEXT NOT NULL DEFAULT '[]',
                Type       INTEGER NOT NULL DEFAULT 0,
                IsSystem   INTEGER NOT NULL DEFAULT 0,
                CreatedAt  TEXT NOT NULL,
                UpdatedAt  TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ContactPhoneNumbers (
                Uid        TEXT NOT NULL PRIMARY KEY,
                ContactUid TEXT NOT NULL REFERENCES Contacts(Uid) ON DELETE CASCADE,
                Number     TEXT NOT NULL,
                IsPrimary  INTEGER NOT NULL DEFAULT 0,
                Tags       TEXT NOT NULL DEFAULT '[]',
                Type       INTEGER NOT NULL DEFAULT 0,
                IsSystem   INTEGER NOT NULL DEFAULT 0,
                CreatedAt  TEXT NOT NULL,
                UpdatedAt  TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ContactAddresses (
                Uid        TEXT NOT NULL PRIMARY KEY,
                ContactUid TEXT NOT NULL REFERENCES Contacts(Uid) ON DELETE CASCADE,
                Street     TEXT NOT NULL,
                City       TEXT NOT NULL,
                State      TEXT NOT NULL DEFAULT '',
                PostalCode TEXT NOT NULL DEFAULT '',
                Country    TEXT NOT NULL,
                IsPrimary  INTEGER NOT NULL DEFAULT 0,
                Tags       TEXT NOT NULL DEFAULT '[]',
                Type       INTEGER NOT NULL DEFAULT 0,
                IsSystem   INTEGER NOT NULL DEFAULT 0,
                CreatedAt  TEXT NOT NULL,
                UpdatedAt  TEXT NOT NULL
            );
            """);

        _logger.Information("Database schema initialized.");
    }

    /// <summary>
    /// Seeds the default admin user if no admin user exists.
    /// </summary>
    /// <param name="adminPasswordHash">The BCrypt-hashed password for the admin user.</param>
    public void SeedAdminUser(string adminPasswordHash)
    {
        using var connection = _connectionFactory.CreateConnection();

        var exists = connection.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM Users WHERE Role = @Role",
            new { Role = (int)UserRole.Admin }) > 0;

        if (exists)
        {
            _logger.Debug("Admin user already exists. Skipping seed.");
            return;
        }

        var now = DateTime.UtcNow;
        var admin = new User
        {
            Uid = Guid.CreateVersion7(),
            Username = AppConstants.DefaultAdminUsername,
            PasswordHash = adminPasswordHash,
            Email = "admin@appsimple.local",
            Role = UserRole.Admin,
            IsActive = true,
            IsSystem = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        connection.Execute("""
            INSERT INTO Users (Uid, Username, PasswordHash, Email, Role, IsActive, IsSystem, CreatedAt, UpdatedAt)
            VALUES (@Uid, @Username, @PasswordHash, @Email, @Role, @IsActive, @IsSystem, @CreatedAt, @UpdatedAt)
            """, new
        {
            Uid = admin.Uid.ToString(),
            admin.Username,
            admin.PasswordHash,
            admin.Email,
            Role = (int)admin.Role,
            IsActive = admin.IsActive ? 1 : 0,
            IsSystem = admin.IsSystem ? 1 : 0,
            CreatedAt = admin.CreatedAt.ToString("O"),
            UpdatedAt = admin.UpdatedAt.ToString("O")
        });

        _logger.Information("Default admin user seeded.");
        SeedDefaultTagsForUser(admin.Uid);
    }

    /// <summary>
    /// Seeds the default set of tags for the given user if they have none yet.
    /// Idempotent — safe to call multiple times.
    /// </summary>
    public void SeedDefaultTagsForUser(Guid userUid)
    {
        using var connection = _connectionFactory.CreateConnection();

        var hasAny = connection.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM Tags WHERE UserUid = @UserUid",
            new { UserUid = userUid.ToString() }) > 0;

        if (hasAny) return;

        var defaultTags = new (string Name, string Color)[]
        {
            ("Default",   "#CCCCCC"),
            ("Personal",  "#A8E6A3"),
            ("Work",      "#4A9EFF"),
            ("Important", "#FF6B6B"),
            ("Later",     "#FFD93D"),
            ("Archive",   "#B0B0B0"),
            ("Shared",    "#96CEB4"),
            ("Private",   "#C7A8FF"),
            ("Urgent",    "#FF4444"),
            ("Follow-up", "#FFB347"),
        };

        var now = DateTime.UtcNow.ToString("O");
        foreach (var (name, color) in defaultTags)
        {
            connection.Execute("""
                INSERT INTO Tags (Uid, UserUid, Name, Color, IsSystem, CreatedAt, UpdatedAt)
                VALUES (@Uid, @UserUid, @Name, @Color, 1, @Now, @Now)
                """, new { Uid = Guid.CreateVersion7().ToString(), UserUid = userUid.ToString(), Name = name, Color = color, Now = now });
        }

        _logger.Information("Seeded default tags for user {UserUid}.", userUid);
    }
}
