using System.Security.Claims;
using AppSimple.WebApp.Models;
using AppSimple.WebApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace AppSimple.WebApp.Controllers;

/// <summary>Controller for authentication (login/logout).</summary>
public sealed class AuthController : Controller
{
    private readonly IApiClient _api;
    private readonly ILogger<AuthController> _logger;

    /// <summary>Initializes a new instance of <see cref="AuthController"/>.</summary>
    public AuthController(IApiClient api, ILogger<AuthController> logger)
    {
        _api = api;
        _logger = logger;
    }

    /// <summary>Displays the login form.</summary>
    [HttpGet("/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return Redirect(returnUrl ?? "/");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    /// <summary>Processes the login form submission.</summary>
    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var result = await _api.LoginAsync(model.Username, model.Password);
            if (result is null)
            {
                model.Error = "Invalid username or password.";
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, result.Username),
                new(ClaimTypes.Role, result.Role),
                new("jwt_token", result.Token)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = true });

            _logger.LogInformation("User {Username} logged in", result.Username);

            return Redirect(model.ReturnUrl ?? "/");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "WebApi unavailable during login");
            model.Error = "The server is currently unavailable. Please try again later.";
            return View(model);
        }
    }

    /// <summary>Logs out the current user.</summary>
    [HttpPost("/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}
