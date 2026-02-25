using AppSimple.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace AppSimple.WebApp.Controllers;

/// <summary>Controller for the home page.</summary>
public sealed class HomeController : Controller
{
    /// <summary>Displays the home page.</summary>
    [HttpGet("/")]
    public IActionResult Index()
    {
        return View(new HomeViewModel
        {
            IsLoggedIn = User.Identity?.IsAuthenticated ?? false,
            Username = User.Identity?.Name,
            IsAdmin = User.IsInRole("Admin")
        });
    }
}
