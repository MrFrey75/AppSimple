namespace AppSimple.Core.Models.DTOs;

/// <summary>Result returned by the WebApi login endpoint.</summary>
public sealed class LoginResult
{
    /// <summary>Gets or sets the JWT bearer token.</summary>
    public required string Token { get; set; }

    /// <summary>Gets or sets the authenticated username.</summary>
    public required string Username { get; set; }

    /// <summary>Gets or sets the user role string (e.g. "Admin" or "User").</summary>
    public required string Role { get; set; }
}
