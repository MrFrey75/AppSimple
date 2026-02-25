using AppSimple.Core.Config;

namespace AppSimple.WebApp.Services.Impl;

/// <summary>File-backed implementation of <see cref="IThemeService"/>.</summary>
public sealed class ThemeService : IThemeService
{
    private readonly IAppConfigService _configService;

    /// <summary>Initializes a new instance of <see cref="ThemeService"/>.</summary>
    public ThemeService(IAppConfigService configService)
    {
        _configService = configService;
    }

    /// <inheritdoc/>
    public string GetCurrentTheme() => _configService.Load().SelectedTheme;

    /// <inheritdoc/>
    public string GetCurrentThemeCss() => ThemeDefinitions.GetCss(GetCurrentTheme());

    /// <inheritdoc/>
    public void SetTheme(string themeName)
    {
        if (!ThemeDefinitions.Names.Contains(themeName)) return;
        _configService.SetSelectedTheme(themeName);
    }
}
