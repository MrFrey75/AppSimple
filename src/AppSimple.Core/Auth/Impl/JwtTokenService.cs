using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AppSimple.Core.Auth;
using AppSimple.Core.Logging;
using AppSimple.Core.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AppSimple.Core.Auth.Impl;

/// <summary>
/// Generates and validates HMAC-SHA256 signed JWT tokens.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly JwtSecurityTokenHandler _handler = new();
    private readonly IAppLogger<JwtTokenService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="JwtTokenService"/>.
    /// </summary>
    public JwtTokenService(IOptions<JwtOptions> options, IAppLogger<JwtTokenService> logger)
    {
         _logger = logger;
         _logger.Debug("JwtTokenService initializing with options: {@Options}", options.Value);
         _options = options.Value;
    }

    /// <inheritdoc />
    public string GenerateToken(User user)
    {
        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Uid.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role,               user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.CreateVersion7().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             _options.Issuer,
            audience:           _options.Audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            signingCredentials: credentials);

        _logger.Debug("Generated JWT token for user '{Username}' with UID {Uid}", user.Username, user.Uid);

        return _handler.WriteToken(token);
    }

    /// <inheritdoc />
    public string? GetUsernameFromToken(string token)
    {
        var principal = Validate(token);
        var username = principal?.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value
                    ?? principal?.FindFirst(ClaimTypes.Name)?.Value;
        _logger.Debug("Extracted username '{Username}' from token", username);
        return username;    
}

    /// <inheritdoc />
    public bool IsTokenValid(string token)
        => Validate(token) is not null;

    private ClaimsPrincipal? Validate(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = key,
            ValidateIssuer           = true,
            ValidIssuer              = _options.Issuer,
            ValidateAudience         = true,
            ValidAudience            = _options.Audience,
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.Zero
        };

        try
        {
            return _handler.ValidateToken(token, parameters, out _);
        }
        catch (Exception ex)
        {
            _logger.Warning("Token validation failed: {Message}", ex.Message);
            return null;
        }
    }
}
