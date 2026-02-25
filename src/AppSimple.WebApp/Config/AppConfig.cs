namespace AppSimple.WebApp.Config;

/// <summary>Application-level configuration persisted to <c>config.json</c>.</summary>
public sealed class AppConfig
{
    /// <summary>Gets or sets the factory-default theme name.</summary>
    public string DefaultTheme { get; set; } = "CatppuccinMocha";

    /// <summary>Gets or sets the user-selected theme name.</summary>
    public string SelectedTheme { get; set; } = "CatppuccinMocha";
}
