namespace AppSimple.Core.Models.Requests;

/// <summary>
/// Request model for updating a user's profile information.
/// All fields are optional — only non-null values are applied.
/// Validated by <c>UpdateUserRequestValidator</c>.
/// </summary>
public sealed class UpdateUserRequest
{
    /// <summary>Gets or sets the updated first name, or <c>null</c> to leave unchanged.</summary>
    public string? FirstName { get; set; }

    /// <summary>Gets or sets the updated last name, or <c>null</c> to leave unchanged.</summary>
    public string? LastName { get; set; }

    /// <summary>Gets or sets the updated phone number, or <c>null</c> to leave unchanged.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Gets or sets the updated date of birth in UTC, or <c>null</c> to leave unchanged.</summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>Gets or sets the updated bio, or <c>null</c> to leave unchanged.</summary>
    public string? Bio { get; set; }

    /// <summary>Gets or sets the updated avatar URL, or <c>null</c> to leave unchanged.</summary>
    public string? AvatarUrl { get; set; }
}
