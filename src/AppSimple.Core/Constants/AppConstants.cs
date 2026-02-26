namespace AppSimple.Core.Constants;

/// <summary>
/// Application-wide string and numeric constants.
/// </summary>
public static class AppConstants
{
    // ── Identity ─────────────────────────────────────────────────────────────

    /// <summary>The name of the application.</summary>
    public const string AppName = "AppSimple";

    /// <summary>The default admin username used during database seeding.</summary>
    public const string DefaultAdminUsername = "admin";

    /// <summary>The default admin password used during initial seeding and database reset.</summary>
    public const string DefaultAdminPassword = "Admin123!";

    /// <summary>The default password for seeded sample users (alice, bob, carol).</summary>
    public const string DefaultSamplePassword = "Sample123!";

    // ── Networking ───────────────────────────────────────────────────────────

    /// <summary>The default base URL for the WebApi, used as a fallback when config is absent.</summary>
    public const string DefaultWebApiBaseUrl = "http://localhost:5157";

    // ── JWT ──────────────────────────────────────────────────────────────────

    /// <summary>The default JWT token lifetime in minutes.</summary>
    public const int DefaultJwtExpirationMinutes = 480;

    // ── Configuration keys ───────────────────────────────────────────────────

    /// <summary>Config key: whether file logging is enabled.</summary>
    public const string ConfigLoggingEnableFile = "AppLogging:EnableFile";

    /// <summary>Config key: directory for log files.</summary>
    public const string ConfigLoggingDirectory = "AppLogging:LogDirectory";

    /// <summary>Config key: SQLite connection string.</summary>
    public const string ConfigDatabaseConnectionString = "Database:ConnectionString";

    /// <summary>Config key: WebApi base URL.</summary>
    public const string ConfigWebApiBaseUrl = "WebApi:BaseUrl";

    /// <summary>Config key: JWT signing secret.</summary>
    public const string ConfigJwtSecret = "Jwt:Secret";

    /// <summary>Config key: JWT issuer claim.</summary>
    public const string ConfigJwtIssuer = "Jwt:Issuer";

    /// <summary>Config key: JWT audience claim.</summary>
    public const string ConfigJwtAudience = "Jwt:Audience";

    /// <summary>Config key: JWT token lifetime in minutes.</summary>
    public const string ConfigJwtExpiration = "Jwt:ExpirationMinutes";

    // ── Default tags ─────────────────────────────────────────────────────────

    /// <summary>
    /// The set of default tags seeded for every new user.
    /// Each entry is (Name, HexColor).
    /// </summary>
    public static readonly IReadOnlyList<(string Name, string Color)> DefaultTags =
    [
        ("Work",      "#4A9EFF"),
        ("Personal",  "#A8E6A3"),
        ("Important", "#FF6B6B"),
        ("Todo",      "#FFD93D"),
        ("Ideas",     "#C7A8FF"),
        ("Learning",  "#FFB347"),
        ("Finance",   "#96CEB4"),
        ("Health",    "#F8B4B4"),
    ];

    /// <summary>The minimum password length enforced by validation.</summary>
    public const int MinPasswordLength = 8;

    /// <summary>The maximum username length enforced by validation.</summary>
    public const int MaxUsernameLength = 50;

    /// <summary>The maximum email length enforced by validation.</summary>
    public const int MaxEmailLength = 256;

    /// <summary>The maximum length for first or last name enforced by validation.</summary>
    public const int MaxNameLength = 100;

    /// <summary>The maximum phone number length enforced by validation.</summary>
    public const int MaxPhoneLength = 30;

    /// <summary>The maximum bio length enforced by validation.</summary>
    public const int MaxBioLength = 500;
}
