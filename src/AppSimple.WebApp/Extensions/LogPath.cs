namespace AppSimple.WebApp.Extensions;

/// <summary>Resolves the log directory path from config, env var, or default location.</summary>
internal static class LogPath
{
    /// <summary>Returns the resolved log directory, creating it if necessary.</summary>
    public static string Resolve(string? configValue = null)
    {
        if (!string.IsNullOrWhiteSpace(configValue)) return configValue;
        var envPath = Environment.GetEnvironmentVariable("APPSIMPLE_LOGS");
        if (!string.IsNullOrWhiteSpace(envPath)) return envPath;
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder = Path.Combine(appData, "AppSimple", "logs");
        Directory.CreateDirectory(folder);
        return folder;
    }
}
