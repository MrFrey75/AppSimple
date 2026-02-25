using AppSimple.Core.Models.Requests;
using AppSimple.Core.Services;
using AppSimple.WebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppSimple.WebApi.Controllers;

/// <summary>
/// Protected endpoint accessible by any authenticated user.
/// </summary>
[ApiController]
[Route("api/protected")]
[Authorize]
public sealed class ProtectedController : ControllerBase
{
    private readonly IUserService _users;

    /// <summary>Initializes a new instance of <see cref="ProtectedController"/>.</summary>
    public ProtectedController(IUserService users)
    {
        _users = users;
    }

    /// <summary>Returns a message confirming the caller is authenticated.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Get()
    {
        var username = User.Identity?.Name ?? "unknown";
        return Ok(new { message = $"Hello, {username}! You are authenticated." });
    }

    /// <summary>Returns the authenticated user's own profile.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me()
    {
        var username = User.Identity?.Name;
        if (username is null) return Unauthorized();

        var user = await _users.GetByUsernameAsync(username);
        if (user is null) return NotFound();

        return Ok(UserDto.From(user));
    }

    /// <summary>Updates the authenticated user's own profile.</summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserRequest request)
    {
        var username = User.Identity?.Name;
        if (username is null) return Unauthorized();

        var user = await _users.GetByUsernameAsync(username);
        if (user is null) return NotFound();

        // Users may only update their own non-privileged fields
        if (request.FirstName   is not null) user.FirstName   = request.FirstName;
        if (request.LastName    is not null) user.LastName    = request.LastName;
        if (request.PhoneNumber is not null) user.PhoneNumber = request.PhoneNumber;
        if (request.Bio         is not null) user.Bio         = request.Bio;
        if (request.DateOfBirth is not null) user.DateOfBirth = request.DateOfBirth;

        await _users.UpdateAsync(user);
        var updated = await _users.GetByUsernameAsync(username);
        return Ok(UserDto.From(updated!));
    }

    /// <summary>Changes the authenticated user's password.</summary>
    [HttpPost("me/change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var username = User.Identity?.Name;
        if (username is null) return Unauthorized();

        var user = await _users.GetByUsernameAsync(username);
        if (user is null) return NotFound();

        await _users.ChangePasswordAsync(user.Uid, request.CurrentPassword, request.NewPassword);
        return NoContent();
    }
}
