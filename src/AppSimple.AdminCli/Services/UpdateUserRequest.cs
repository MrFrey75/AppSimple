namespace AppSimple.AdminCli.Services;

/// <summary>Request payload for updating a user via the WebApi.</summary>
public sealed class UpdateUserRequest
{
    /// <summary>Gets or sets the first name.</summary>
    public string? FirstName { get; set; }

    /// <summary>Gets or sets the last name.</summary>
    public string? LastName { get; set; }

    /// <summary>Gets or sets the phone number.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Gets or sets the biography.</summary>
    public string? Bio { get; set; }

    /// <summary>Gets or sets the date of birth.</summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>Gets or sets the role (0 = User, 1 = Admin).</summary>
    public int? Role { get; set; }

    /// <summary>Gets or sets a value indicating whether the account is active.</summary>
    public bool? IsActive { get; set; }
}
