using AppSimple.Core.Enums;
using AppSimple.Core.Models.Requests;
using System.Security.Claims;
using AppSimple.WebApp.Models;
using AppSimple.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppSimple.WebApp.Controllers;

/// <summary>Controller for the admin user management pages.</summary>
[Authorize(Roles = "Admin")]
[Route("admin")]
public sealed class AdminController : Controller
{
    private readonly IApiClient _api;
    private readonly ILogger<AdminController> _logger;

    /// <summary>Initializes a new instance of <see cref="AdminController"/>.</summary>
    public AdminController(IApiClient api, ILogger<AdminController> logger)
    {
        _api = api;
        _logger = logger;
    }

    private string? GetToken() => User.FindFirstValue("jwt_token");

    /// <summary>Displays the user list.</summary>
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var users = await _api.GetAllUsersAsync(token);
        return View(new UserListViewModel { Users = users });
    }

    /// <summary>Displays the create user form.</summary>
    [HttpGet("create")]
    public IActionResult Create() => View(new CreateUserViewModel());

    /// <summary>Processes the create user form submission.</summary>
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var result = await _api.CreateUserAsync(token, model.Username, model.Email, model.Password);
        if (result is null)
        {
            _logger.LogWarning("Admin '{Admin}' failed to create user '{Username}'", User.Identity?.Name, model.Username);
            model.Error = "Failed to create user. The username or email may already be taken.";
            return View(model);
        }

        _logger.LogInformation("Admin '{Admin}' created user '{Username}'", User.Identity?.Name, result.Username);
        TempData["Success"] = $"User '{result.Username}' created successfully.";
        return RedirectToAction("Index");
    }

    /// <summary>Displays the edit user form.</summary>
    [HttpGet("edit/{uid:guid}")]
    public async Task<IActionResult> Edit(Guid uid)
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var user = await _api.GetUserAsync(token, uid);
        if (user is null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction("Index");
        }

        return View(new EditUserViewModel
        {
            Uid = user.Uid,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Bio = user.Bio,
            DateOfBirth = user.DateOfBirth,
            Role = user.Role,
            IsActive = user.IsActive
        });
    }

    /// <summary>Processes the edit user form submission.</summary>
    [HttpPost("edit/{uid:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid uid, EditUserViewModel model)
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
            DateOfBirth = model.DateOfBirth,
            Role = (UserRole)model.Role,
            IsActive = model.IsActive
        };

        var result = await _api.UpdateUserAsync(token, uid, request);
        if (result is null)
        {
            _logger.LogWarning("Admin '{Admin}' failed to update user ({Uid})", User.Identity?.Name, uid);
            model.Error = "Failed to update user.";
            return View(model);
        }

        _logger.LogInformation("Admin '{Admin}' updated user '{Username}' ({Uid})", User.Identity?.Name, result.Username, uid);
        TempData["Success"] = $"User '{result.Username}' updated successfully.";
        return RedirectToAction("Index");
    }

    /// <summary>Deletes a user.</summary>
    [HttpPost("delete/{uid:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid uid)
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var success = await _api.DeleteUserAsync(token, uid);
        if (success)
            _logger.LogInformation("Admin '{Admin}' deleted user ({Uid})", User.Identity?.Name, uid);
        else
            _logger.LogWarning("Admin '{Admin}' failed to delete user ({Uid})", User.Identity?.Name, uid);

        TempData[success ? "Success" : "Error"] = success
            ? "User deleted successfully."
            : "Failed to delete user.";

        return RedirectToAction("Index");
    }
}
