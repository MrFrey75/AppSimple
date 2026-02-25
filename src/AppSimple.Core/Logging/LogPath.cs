namespace AppSimple.Core.Logging;

/// <summary>
/// Resolves the shared log directory for all AppSimple applications.
/// </summary>
/// <remarks>
/// Default location: <c>~/.local/share/AppSimple/logs</c> (Linux/macOS)
/// or <c>%LOCALAPPDATA%\AppSimple\logs</c> (Windows).
/// <para>
/// Override with the <c>APPSIMPLE_LOGS</c> environment variable or via
/// <c>AppLogging:LogDirectory</c> in <c>appsettings.json</c>.
/// </para>
/// </remarks>
public static class LogPath
{
    private const string AppFolder  = "AppSimple";
    private const string LogsFolder = "logs";

    /// <summary>
    /// Returns the resolved log directory path.
    /// Priority: explicit config value → APPSIMPLE_LOGS env var → shared OS default.
    /// </summary>
    /// <param name="configValue">
    /// The <c>AppLogging:LogDirectory</c> value from <c>appsettings.json</c>.
    /// Pass <c>null</c> or empty to use the default shared location.
    /// </param>
    public static string Resolve(string? configValue = null)
    {
        // 1. Explicit config value wins
        if (!string.IsNullOrWhiteSpace(configValue))
            return configValue;

        // 2. Environment variable override
        var envPath = Environment.GetEnvironmentVariable("APPSIMPLE_LOGS");
        if (!string.IsNullOrWhiteSpace(envPath))
            return envPath;

        // 3. Default shared location alongside the database
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder  = Path.Combine(appData, AppFolder, LogsFolder);
        Directory.CreateDirectory(folder);
        return folder;
    }
}
