namespace AppSimple.Core.Auth;

/// <summary>
/// Abstraction for hashing and verifying passwords.
/// The default implementation uses BCrypt.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain-text password using a secure one-way algorithm.
    /// </summary>
    /// <param name="plainPassword">The password to hash.</param>
    /// <returns>The hashed password string suitable for storage.</returns>
    string Hash(string plainPassword);

    /// <summary>
    /// Verifies a plain-text password against a stored hash.
    /// </summary>
    /// <param name="plainPassword">The plain-text candidate password.</param>
    /// <param name="hash">The stored hash to verify against.</param>
    /// <returns><c>true</c> if the password matches; otherwise <c>false</c>.</returns>
    bool Verify(string plainPassword, string hash);
}
