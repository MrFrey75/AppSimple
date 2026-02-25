namespace AppSimple.WebApp.Services;

/// <summary>Manages the active theme for the WebApp.</summary>
public interface IThemeService
{
    /// <summary>Returns the currently selected theme name.</summary>
    string GetCurrentTheme();

    /// <summary>Returns the CSS variable block for the current theme.</summary>
    string GetCurrentThemeCss();

    /// <summary>Sets the active theme and persists it to config.json.</summary>
    void SetTheme(string themeName);
}
