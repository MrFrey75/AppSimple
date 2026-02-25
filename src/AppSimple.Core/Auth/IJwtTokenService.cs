using AppSimple.Core.Models;

namespace AppSimple.Core.Auth;

/// <summary>
/// Abstraction for generating and validating JWT tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a signed JWT token for the given user.
    /// </summary>
    /// <param name="user">The authenticated user whose claims are embedded in the token.</param>
    /// <returns>A signed JWT token string.</returns>
    string GenerateToken(User user);

    /// <summary>
    /// Validates a JWT token and returns the username encoded within it.
    /// </summary>
    /// <param name="token">The JWT token string to validate.</param>
    /// <returns>The username claim if the token is valid and not expired; otherwise <c>null</c>.</returns>
    string? GetUsernameFromToken(string token);

    /// <summary>
    /// Checks whether a given JWT token is structurally valid, properly signed, and not expired.
    /// </summary>
    bool IsTokenValid(string token);
}
