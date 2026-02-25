using System.ComponentModel.DataAnnotations;

namespace AppSimple.WebApp.Models;

/// <summary>View model for the admin edit user form.</summary>
public sealed class EditUserViewModel
{
    public Guid Uid { get; set; }
    public string Username { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    public int Role { get; set; }
    public bool IsActive { get; set; }
    public string? Error { get; set; }
}
