namespace AppSimple.Core.Services;

/// <summary>
/// Defines authentication operations for user login and token management.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user using their username and plain-text password.
    /// </summary>
    /// <param name="username">The username of the user attempting to log in.</param>
    /// <param name="plainPassword">The plain-text password to verify.</param>
    /// <returns>The signed JWT token string on success.</returns>
    /// <exception cref="AppSimple.Core.Common.Exceptions.UnauthorizedException">
    /// Thrown when the credentials are invalid or the account is inactive.
    /// </exception>
    Task<string> LoginAsync(string username, string plainPassword);

    /// <summary>
    /// Validates a JWT token and returns the username encoded within it.
    /// </summary>
    /// <param name="token">The JWT token string to validate.</param>
    /// <returns>The username if the token is valid; otherwise <c>null</c>.</returns>
    string? ValidateToken(string token);
}
