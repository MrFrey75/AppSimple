namespace AppSimple.DataLib.Db;

/// <summary>
/// Resolves the shared SQLite database path for all AppSimple applications.
/// </summary>
/// <remarks>
/// Default location: <c>~/.local/share/AppSimple/appsimple.db</c> (Linux/macOS)
/// or <c>%LOCALAPPDATA%\AppSimple\appsimple.db</c> (Windows).
/// <para>
/// Override with the <c>APPSIMPLE_DB</c> environment variable or via
/// <c>Database:ConnectionString</c> in <c>appsettings.json</c>.
/// </para>
/// </remarks>
public static class DatabasePath
{
    private const string AppFolder = "AppSimple";
    private const string DbFile    = "appsimple.db";

    /// <summary>
    /// Returns the resolved SQLite connection string.
    /// Priority: explicit config value → APPSIMPLE_DB env var → shared default path.
    /// </summary>
    /// <param name="configValue">
    /// The <c>Database:ConnectionString</c> value from <c>appsettings.json</c>.
    /// Pass <c>null</c> or empty to use the default shared location.
    /// </param>
    public static string Resolve(string? configValue = null)
    {
        // 1. Explicit config value wins (must be a full connection string)
        if (!string.IsNullOrWhiteSpace(configValue))
            return configValue;

        // 2. Environment variable override (just the file path)
        var envPath = Environment.GetEnvironmentVariable("APPSIMPLE_DB");
        if (!string.IsNullOrWhiteSpace(envPath))
            return $"Data Source={envPath}";

        // 3. Default shared location
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder  = Path.Combine(appData, AppFolder);
        Directory.CreateDirectory(folder);

        var defaultPath = Path.Combine(folder, DbFile);

        return $"Data Source={defaultPath}";
    }

    /// <summary>Gets the resolved absolute path to the database file.</summary>
    public static string FilePath(string? configValue = null)
    {
        var cs = Resolve(configValue);
        // Extract path from "Data Source=..." connection string
        var idx = cs.IndexOf('=');
        return idx >= 0 ? cs[(idx + 1)..].Trim() : cs;
    }
}
