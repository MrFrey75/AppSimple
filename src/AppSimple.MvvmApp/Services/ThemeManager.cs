using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using AppSimple.Core.Config;

namespace AppSimple.MvvmApp.Services;

/// <summary>Manages runtime Avalonia theme switching by swapping merged ResourceDictionary entries.</summary>
public sealed class ThemeManager
{
    private const string UriBase = "avares://AppSimple.MvvmApp/Themes/";

    private readonly IAppConfigService _configService;

    /// <summary>All supported theme names in display order.</summary>
    public static readonly IReadOnlyList<string> Themes =
    [
        "CatppuccinMocha",
        "CatppuccinLatte",
        "Dracula",
        "Nord",
        "SolarizedLight"
    ];

    /// <summary>Display-friendly labels matching <see cref="Themes"/> order.</summary>
    public static readonly IReadOnlyList<string> ThemeLabels =
    [
        "Catppuccin Mocha",
        "Catppuccin Latte",
        "Dracula",
        "Nord",
        "Solarized Light"
    ];

    /// <summary>Initializes a new instance of <see cref="ThemeManager"/>.</summary>
    public ThemeManager(IAppConfigService configService)
    {
        _configService = configService;
    }

    /// <summary>Applies the saved theme from config. Call once at startup after Avalonia is initialized.</summary>
    public void ApplySavedTheme()
    {
        var theme = _configService.GetSelectedTheme();
        Apply(theme, save: false);
    }

    /// <summary>Switches to the named theme and persists the selection.</summary>
    public void SetTheme(string themeName)
    {
        Apply(themeName, save: true);
    }

    private void Apply(string themeName, bool save)
    {
        if (!Themes.Contains(themeName))
            themeName = "CatppuccinMocha";

        var uri = new Uri($"{UriBase}{themeName}.axaml");
        var dict = new ResourceInclude(uri) { Source = uri };

        var app = Application.Current!;
        // Replace first merged dictionary (our theme slot)
        if (app.Resources.MergedDictionaries.Count > 0)
            app.Resources.MergedDictionaries[0] = dict;
        else
            app.Resources.MergedDictionaries.Add(dict);

        if (save)
            _configService.SetSelectedTheme(themeName);
    }
}
