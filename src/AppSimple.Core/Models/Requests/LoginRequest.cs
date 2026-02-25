namespace AppSimple.Core.Models.Requests;

/// <summary>Request model for authenticating a user.</summary>
public sealed class LoginRequest
{
    /// <summary>Gets or sets the username.</summary>
    public required string Username { get; set; }

    /// <summary>Gets or sets the plain-text password.</summary>
    public required string Password { get; set; }
}
