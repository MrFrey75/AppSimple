namespace AppSimple.WebApp.Services;

/// <summary>Request body for updating a user profile via the WebApi.</summary>
public sealed class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? Role { get; set; }
    public bool? IsActive { get; set; }
}
