namespace AppSimple.WebApp.Services;

/// <summary>Data transfer object representing a user from the WebApi.</summary>
public sealed class UserDto
{
    public Guid Uid { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
