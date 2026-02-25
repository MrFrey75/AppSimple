namespace AppSimple.WebApi.DTOs;

/// <summary>Request body for creating a new user.</summary>
public sealed class CreateUserRequest
{
    /// <summary>Gets or sets the desired username.</summary>
    public required string Username { get; set; }

    /// <summary>Gets or sets the user's email address.</summary>
    public required string Email { get; set; }

    /// <summary>Gets or sets the plain-text password.</summary>
    public required string Password { get; set; }
}
