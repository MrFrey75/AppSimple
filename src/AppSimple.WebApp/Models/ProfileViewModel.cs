namespace AppSimple.WebApp.Models;

/// <summary>View model for the profile display page.</summary>
public sealed class ProfileViewModel
{
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string? FullName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Role { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
