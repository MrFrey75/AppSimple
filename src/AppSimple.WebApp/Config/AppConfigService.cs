using System.Text.Json;

namespace AppSimple.WebApp.Config;

/// <summary>File-backed service that reads and writes <c>config.json</c>.</summary>
public sealed class AppConfigService
{
    private static readonly JsonSerializerOptions _opts = new() { WriteIndented = true };
    private readonly string _path;

    /// <summary>Initializes a new instance of <see cref="AppConfigService"/> with the resolved config file path.</summary>
    public AppConfigService(string path)
    {
        _path = path;
    }

    /// <summary>Loads the current configuration, creating defaults if the file does not exist.</summary>
    public AppConfig Load()
    {
        if (!File.Exists(_path))
        {
            var defaults = new AppConfig();
            Save(defaults);
            return defaults;
        }
        try
        {
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<AppConfig>(json, _opts) ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
    }

    /// <summary>Persists the given configuration to disk.</summary>
    public void Save(AppConfig config)
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(_path, JsonSerializer.Serialize(config, _opts));
    }

    /// <summary>Gets the currently selected theme name.</summary>
    public string GetSelectedTheme() => Load().SelectedTheme;

    /// <summary>Sets the selected theme name and saves it.</summary>
    public void SetSelectedTheme(string themeName)
    {
        var config = Load();
        config.SelectedTheme = themeName;
        Save(config);
    }
}
