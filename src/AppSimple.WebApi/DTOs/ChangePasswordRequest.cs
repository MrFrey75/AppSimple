namespace AppSimple.WebApi.DTOs;

/// <summary>Request body for changing the authenticated user's password.</summary>
public sealed class ChangePasswordRequest
{
    /// <summary>Gets or sets the current (existing) password.</summary>
    public required string CurrentPassword { get; set; }

    /// <summary>Gets or sets the new password.</summary>
    public required string NewPassword { get; set; }
}
