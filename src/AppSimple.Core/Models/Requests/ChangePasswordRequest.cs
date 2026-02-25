namespace AppSimple.Core.Models.Requests;

/// <summary>
/// Request model for changing a user's password.
/// Validated by <c>ChangePasswordRequestValidator</c>.
/// </summary>
public sealed class ChangePasswordRequest
{
    /// <summary>Gets or sets the user's current password, used for verification before the change.</summary>
    public required string CurrentPassword { get; set; }

    /// <summary>Gets or sets the new plain-text password.</summary>
    public required string NewPassword { get; set; }

    /// <summary>Gets or sets the confirmation of the new password. Must match <see cref="NewPassword"/>. Optional for API callers.</summary>
    public string? ConfirmNewPassword { get; set; }
}
