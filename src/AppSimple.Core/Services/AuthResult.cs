namespace AppSimple.Core.Services;

/// <summary>
/// Result returned by authentication operations.
/// </summary>
public sealed class AuthResult
{
    /// <summary>Gets or sets a value indicating whether authentication succeeded.</summary>
    public required bool Succeeded { get; set; }

    /// <summary>Gets or sets the JWT token string when authentication succeeds; otherwise <c>null</c>.</summary>
    public string? Token { get; set; }

    /// <summary>Gets or sets a human-readable message describing the result.</summary>
    public required string Message { get; set; }

    /// <summary>Creates a successful <see cref="AuthResult"/> with the given token.</summary>
    public static AuthResult Success(string token) =>
        new() { Succeeded = true, Token = token, Message = "Authentication successful." };

    /// <summary>Creates a failed <see cref="AuthResult"/> with the given error message.</summary>
    public static AuthResult Failure(string message) =>
        new() { Succeeded = false, Message = message };
}
