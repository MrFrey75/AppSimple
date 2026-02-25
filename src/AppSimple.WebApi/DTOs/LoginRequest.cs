namespace AppSimple.WebApi.DTOs;

/// <summary>Request body for the login endpoint.</summary>
public sealed class LoginRequest
{
    /// <summary>Gets or sets the username.</summary>
    public required string Username { get; set; }

    /// <summary>Gets or sets the plain-text password.</summary>
    public required string Password { get; set; }
}
