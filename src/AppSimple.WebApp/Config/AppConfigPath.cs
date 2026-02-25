namespace AppSimple.WebApp.Config;

/// <summary>Resolves the path to the shared <c>config.json</c> file.</summary>
public static class AppConfigPath
{
    /// <summary>
    /// Returns the full path to <c>config.json</c>, resolved in this priority order:
    /// <list type="number">
    ///   <item>Non-empty <paramref name="configValue"/> passed directly</item>
    ///   <item><c>APPSIMPLE_CONFIG</c> environment variable</item>
    ///   <item><c>~/.local/share/AppSimple/config.json</c> (Linux) / <c>%LOCALAPPDATA%\AppSimple\config.json</c> (Windows)</item>
    /// </list>
    /// </summary>
    public static string Resolve(string? configValue = null)
    {
        if (!string.IsNullOrWhiteSpace(configValue)) return configValue;

        var env = Environment.GetEnvironmentVariable("APPSIMPLE_CONFIG");
        if (!string.IsNullOrWhiteSpace(env)) return env;

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dir = Path.Combine(appData, "AppSimple");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "config.json");
    }
}
