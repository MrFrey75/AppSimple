using System.Text.Json;

namespace AppSimple.Core.Config.Impl;

/// <summary>File-backed implementation of <see cref="IAppConfigService"/>.</summary>
public sealed class AppConfigService : IAppConfigService
{
    private static readonly JsonSerializerOptions _opts = new() { WriteIndented = true };
    private readonly string _path;

    /// <summary>Initializes a new instance of <see cref="AppConfigService"/> with the resolved config file path.</summary>
    public AppConfigService(string path)
    {
        _path = path;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public void Save(AppConfig config)
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(_path, JsonSerializer.Serialize(config, _opts));
    }

    /// <inheritdoc/>
    public string GetSelectedTheme() => Load().SelectedTheme;

    /// <inheritdoc/>
    public void SetSelectedTheme(string themeName)
    {
        var config = Load();
        config.SelectedTheme = themeName;
        Save(config);
    }
}
