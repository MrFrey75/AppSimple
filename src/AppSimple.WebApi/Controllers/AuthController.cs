using AppSimple.Core.Logging;
using AppSimple.Core.Models.Requests;
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
    private readonly IAppLogger<AuthController> _logger;

    /// <summary>Initializes a new instance of <see cref="AuthController"/>.</summary>
    public AuthController(IAuthService auth, IUserService users, IAppLogger<AuthController> logger)
    {
        _auth   = auth;
        _users  = users;
        _logger = logger;
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
        _logger.Debug("Login attempt for '{Username}'", request.Username);

        // Throws UnauthorizedException on bad credentials — caught by ExceptionMiddleware → 401
        var token = await _auth.LoginAsync(request.Username, request.Password);

        var user = await _users.GetByUsernameAsync(request.Username);
        if (user is null)
        {
            _logger.Warning("Login succeeded but user '{Username}' not found in database", request.Username);
            return Unauthorized(new { error = "User not found." });
        }

        _logger.Information("User '{Username}' (Role: {Role}) logged in", user.Username, user.Role);

        return Ok(new LoginResponse
        {
            Token    = token,
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
        {
            _logger.Debug("Token validation failed");
            return BadRequest(new { error = "Token is invalid or expired." });
        }

        _logger.Debug("Token validated for '{Username}'", username);
        return Ok(new { username, valid = true });
    }
}
