using System.ComponentModel.DataAnnotations;

namespace AppSimple.WebApp.Models;

/// <summary>View model for the edit profile form.</summary>
public sealed class EditProfileViewModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }
}
