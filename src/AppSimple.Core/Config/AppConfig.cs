namespace AppSimple.Core.Config;

/// <summary>Application-level configuration persisted to <c>config.json</c> in the shared AppSimple data directory.</summary>
public sealed class AppConfig
{
    /// <summary>Gets or sets the factory-default theme name. Never changed after first write.</summary>
    public string DefaultTheme { get; set; } = "CatppuccinMocha";

    /// <summary>Gets or sets the user-selected theme name.</summary>
    public string SelectedTheme { get; set; } = "CatppuccinMocha";
}
