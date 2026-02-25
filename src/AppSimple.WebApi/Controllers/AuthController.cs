using AppSimple.Core.Services;
using AppSimple.WebApi.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AppSimple.WebApi.Controllers;

/// <summary>
/// Handles user authentication: login and token validation.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService  _auth;
    private readonly IUserService  _users;

    /// <summary>Initializes a new instance of <see cref="AuthController"/>.</summary>
    public AuthController(IAuthService auth, IUserService users)
    {
        _auth  = auth;
        _users = users;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT bearer token on success.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _auth.LoginAsync(request.Username, request.Password);

        if (!result.Succeeded || result.Token is null)
            return Unauthorized(new { error = result.Message });

        var user = await _users.GetByUsernameAsync(request.Username);
        if (user is null)
            return Unauthorized(new { error = "User not found." });

        return Ok(new LoginResponse
        {
            Token    = result.Token,
            Username = user.Username,
            Role     = user.Role.ToString(),
        });
    }

    /// <summary>
    /// Validates a bearer token and returns the embedded username.
    /// </summary>
    /// <param name="token">The JWT token string.</param>
    [HttpGet("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Validate([FromQuery] string token)
    {
        var username = _auth.ValidateToken(token);
        if (username is null)
            return BadRequest(new { error = "Token is invalid or expired." });

        return Ok(new { username, valid = true });
    }
}
