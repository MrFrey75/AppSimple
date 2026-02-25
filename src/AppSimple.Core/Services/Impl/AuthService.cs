using AppSimple.Core.Auth;
using AppSimple.Core.Interfaces;
using AppSimple.Core.Logging;

namespace AppSimple.Core.Services.Impl;

/// <summary>
/// Handles user authentication via password verification and JWT token issuance.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAppLogger<AuthService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="AuthService"/>.
    /// </summary>
    public AuthService(
        IUserRepository    userRepository,
        IPasswordHasher    passwordHasher,
        IJwtTokenService   jwtTokenService,
        IAppLogger<AuthService> logger)
    {
        _userRepository  = userRepository;
        _passwordHasher  = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _logger          = logger;
    }

    /// <inheritdoc />
    public async Task<AuthResult> LoginAsync(string username, string plainPassword)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user is null)
        {
            _logger.Warning("Login failed — user '{Username}' not found.", username);
            return AuthResult.Failure("Invalid username or password.");
        }

        if (!user.IsActive)
        {
            _logger.Warning("Login rejected — user '{Username}' is inactive.", username);
            return AuthResult.Failure("Account is disabled. Please contact an administrator.");
        }

        if (!_passwordHasher.Verify(plainPassword, user.PasswordHash))
        {
            _logger.Warning("Login failed — invalid password for '{Username}'.", username);
            return AuthResult.Failure("Invalid username or password.");
        }

        var token = _jwtTokenService.GenerateToken(user);
        _logger.Information("User '{Username}' authenticated successfully.", username);
        return AuthResult.Success(token);
    }

    /// <inheritdoc />
    public string? ValidateToken(string token)
        => _jwtTokenService.GetUsernameFromToken(token);
}
