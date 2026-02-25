namespace AppSimple.Core.Config;

/// <summary>Reads and writes the application configuration file (<c>config.json</c>).</summary>
public interface IAppConfigService
{
    /// <summary>Loads the current configuration, creating defaults if the file does not exist.</summary>
    AppConfig Load();

    /// <summary>Persists the given configuration to disk.</summary>
    void Save(AppConfig config);

    /// <summary>Gets the currently selected theme name.</summary>
    string GetSelectedTheme();

    /// <summary>Sets the selected theme name and saves it.</summary>
    void SetSelectedTheme(string themeName);
}
