using AppSimple.Core.Enums;

namespace AppSimple.WebApi.DTOs;

/// <summary>Request body for updating a user's profile.</summary>
public sealed class UpdateUserRequest
{
    /// <summary>Gets or sets the user's first name.</summary>
    public string? FirstName { get; set; }

    /// <summary>Gets or sets the user's last name.</summary>
    public string? LastName { get; set; }

    /// <summary>Gets or sets the user's phone number.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Gets or sets the user's biography.</summary>
    public string? Bio { get; set; }

    /// <summary>Gets or sets the user's date of birth.</summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>Gets or sets the role to assign (Admin-only field).</summary>
    public UserRole? Role { get; set; }

    /// <summary>Gets or sets whether the account is active (Admin-only field).</summary>
    public bool? IsActive { get; set; }
}
