using AppSimple.Core.Enums;
using AppSimple.Core.Logging;
using AppSimple.Core.Models.DTOs;
using AppSimple.Core.Models.Requests;
using AppSimple.Core.Services;
using AppSimple.WebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppSimple.WebApi.Controllers;

/// <summary>
/// Admin-only user management endpoints.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminController : ControllerBase
{
    private readonly IUserService _users;
    private readonly IAppLogger<AdminController> _logger;

    /// <summary>Initializes a new instance of <see cref="AdminController"/>.</summary>
    public AdminController(IUserService users, IAppLogger<AdminController> logger)
    {
        _users  = users;
        _logger = logger;
    }

    /// <summary>Returns a message confirming the caller has admin access.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Index() =>
        Ok(new { message = $"Hello, {User.Identity?.Name}! You have admin access." });

    /// <summary>Returns all users.</summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _users.GetAllAsync();
        _logger.Debug("Admin '{Admin}' retrieved all users ({Count} records)", User.Identity?.Name, users.Count());
        return Ok(users.Select(UserDto.From));
    }

    /// <summary>Returns a single user by UID.</summary>
    [HttpGet("users/{uid:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(Guid uid)
    {
        var user = await _users.GetByUidAsync(uid);
        return user is null ? NotFound() : Ok(UserDto.From(user));
    }

    /// <summary>Creates a new user.</summary>
    [HttpPost("users")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = await _users.CreateAsync(request.Username, request.Email, request.Password);
        _logger.Information("Admin '{Admin}' created user '{Username}' ({Uid})", User.Identity?.Name, user.Username, user.Uid);
        return CreatedAtAction(nameof(GetUser), new { uid = user.Uid }, UserDto.From(user));
    }

    /// <summary>Updates a user's profile and/or role/active status.</summary>
    [HttpPut("users/{uid:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(Guid uid, [FromBody] UpdateUserRequest request)
    {
        var user = await _users.GetByUidAsync(uid);
        if (user is null) return NotFound();

        if (request.FirstName   is not null) user.FirstName   = request.FirstName;
        if (request.LastName    is not null) user.LastName    = request.LastName;
        if (request.PhoneNumber is not null) user.PhoneNumber = request.PhoneNumber;
        if (request.Bio         is not null) user.Bio         = request.Bio;
        if (request.DateOfBirth is not null) user.DateOfBirth = request.DateOfBirth;
        if (request.Role        is not null) user.Role        = request.Role.Value;
        if (request.IsActive    is not null) user.IsActive    = request.IsActive.Value;

        await _users.UpdateAsync(user);
        _logger.Information("Admin '{Admin}' updated user '{Username}' ({Uid})", User.Identity?.Name, user.Username, uid);
        return Ok(UserDto.From((await _users.GetByUidAsync(uid))!));
    }

    /// <summary>Deletes a user by UID.</summary>
    [HttpDelete("users/{uid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser(Guid uid)
    {
        await _users.DeleteAsync(uid);
        _logger.Information("Admin '{Admin}' deleted user ({Uid})", User.Identity?.Name, uid);
        return NoContent();
    }

    /// <summary>Sets a user's role.</summary>
    [HttpPatch("users/{uid:guid}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetRole(Guid uid, [FromBody] UserRole role)
    {
        var user = await _users.GetByUidAsync(uid);
        if (user is null) return NotFound();

        user.Role = role;
        await _users.UpdateAsync(user);
        _logger.Information("Admin '{Admin}' set role of user ({Uid}) to {Role}", User.Identity?.Name, uid, role);
        return NoContent();
    }
}
