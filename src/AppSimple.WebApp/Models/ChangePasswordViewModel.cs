using System.ComponentModel.DataAnnotations;

namespace AppSimple.WebApp.Models;

/// <summary>View model for the change password form.</summary>
public sealed class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = "";

    public string? Error { get; set; }
    public string? Success { get; set; }
}
