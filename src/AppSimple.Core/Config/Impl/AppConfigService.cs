using AppSimple.Core.Logging;
using System.Text.Json;

namespace AppSimple.Core.Config.Impl;

/// <summary>File-backed implementation of <see cref="IAppConfigService"/>.</summary>
public sealed class AppConfigService : IAppConfigService
{
    private static readonly JsonSerializerOptions _opts = new() { WriteIndented = true };
    private readonly string _path;
    private readonly IAppLogger<AppConfigService> _logger;

    /// <summary>Initializes a new instance of <see cref="AppConfigService"/> with the resolved config file path.</summary>
    public AppConfigService(string path, IAppLogger<AppConfigService> logger)
    {
        _logger = logger;
        _logger.Debug("AppConfigService initializing with path: {Path}", path);
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
            _logger.Debug("Loaded config from {Path}: {Json}", _path, json);
            return JsonSerializer.Deserialize<AppConfig>(json, _opts) ?? new AppConfig();
        }
        catch
        {
            _logger.Warning("Failed to load config from {Path}, using defaults", _path);    
            return new AppConfig();
        }
    }

    /// <inheritdoc/>
    public void Save(AppConfig config)
    {
        try
        {
            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(_path, JsonSerializer.Serialize(config, _opts));
        }
        catch (System.Exception ex)
        {
            _logger.Warning("Failed to save config to {Path}: {Message}", _path, ex.Message);
        }
    }

    /// <inheritdoc/>
    public string GetSelectedTheme() => Load().SelectedTheme;

    /// <inheritdoc/>
    public void SetSelectedTheme(string themeName)
    {
        try
        {
            var config = Load();
            config.SelectedTheme = themeName;
            Save(config);
            _logger.Debug("Set selected theme to '{Theme}' in config", themeName);
        }
        catch (System.Exception ex)
        {
            _logger.Warning("Failed to set selected theme to '{Theme}': {Message}", themeName, ex.Message);
        }

    }
}
