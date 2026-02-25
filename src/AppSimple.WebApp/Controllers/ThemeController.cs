using AppSimple.WebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppSimple.WebApp.Controllers;

/// <summary>Handles theme selection requests.</summary>
public sealed class ThemeController : Controller
{
    private readonly IThemeService _themeService;

    /// <summary>Initializes a new instance of <see cref="ThemeController"/>.</summary>
    public ThemeController(IThemeService themeService)
    {
        _themeService = themeService;
    }

    /// <summary>Sets the active theme and redirects back to the referring page.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Set(string theme, string returnUrl = "/")
    {
        _themeService.SetTheme(theme);
        return LocalRedirect(returnUrl);
    }
}
