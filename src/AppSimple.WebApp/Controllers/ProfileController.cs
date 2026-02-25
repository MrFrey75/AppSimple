using AppSimple.Core.Enums;
using AppSimple.Core.Models.Requests;
using System.Security.Claims;
using AppSimple.WebApp.Models;
using AppSimple.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppSimple.WebApp.Controllers;

/// <summary>Controller for the user profile pages.</summary>
[Authorize]
[Route("profile")]
public sealed class ProfileController : Controller
{
    private readonly IApiClient _api;
    private readonly ILogger<ProfileController> _logger;

    /// <summary>Initializes a new instance of <see cref="ProfileController"/>.</summary>
    public ProfileController(IApiClient api, ILogger<ProfileController> logger)
    {
        _api = api;
        _logger = logger;
    }

    private string? GetToken() => User.FindFirstValue("jwt_token");

    /// <summary>Displays the user's profile.</summary>
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var user = await _api.GetMeAsync(token);
        if (user is null)
        {
            TempData["Error"] = "Could not load profile.";
            return RedirectToAction("Index", "Home");
        }

        return View(new ProfileViewModel
        {
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Bio = user.Bio,
            DateOfBirth = user.DateOfBirth,
            Role = user.Role == UserRole.Admin ? "Admin" : "User",
            CreatedAt = user.CreatedAt
        });
    }

    /// <summary>Displays the edit profile form.</summary>
    [HttpGet("edit")]
    public async Task<IActionResult> Edit()
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var user = await _api.GetMeAsync(token);
        if (user is null)
        {
            TempData["Error"] = "Could not load profile.";
            return RedirectToAction("Index");
        }

        return View(new EditProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Bio = user.Bio,
            DateOfBirth = user.DateOfBirth
        });
    }

    /// <summary>Processes the edit profile form submission.</summary>
    [HttpPost("edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var request = new UpdateUserRequest
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            Bio = model.Bio,
            DateOfBirth = model.DateOfBirth
        };

        var result = await _api.UpdateMeAsync(token, request);
        if (result is null)
        {
            _logger.LogWarning("Profile update failed for '{Username}'", User.Identity?.Name);
            TempData["Error"] = "Failed to update profile.";
            return View(model);
        }

        _logger.LogInformation("User '{Username}' updated their profile", User.Identity?.Name);
        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction("Index");
    }

    /// <summary>Displays the change password form.</summary>
    [HttpGet("change-password")]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    /// <summary>Processes the change password form submission.</summary>
    [HttpPost("change-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var success = await _api.ChangePasswordAsync(token, model.CurrentPassword, model.NewPassword);
        if (!success)
        {
            _logger.LogWarning("Password change failed for '{Username}'", User.Identity?.Name);
            model.Error = "Failed to change password. Check your current password and try again.";
            return View(model);
        }

        _logger.LogInformation("User '{Username}' changed their password", User.Identity?.Name);
        TempData["Success"] = "Password changed successfully.";
        return RedirectToAction("Index");
    }
}
