using System.ComponentModel.DataAnnotations;

namespace AppSimple.WebApp.Models;

/// <summary>View model for the create user form.</summary>
public sealed class CreateUserViewModel
{
    [Required]
    public string Username { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public string? Error { get; set; }
}
