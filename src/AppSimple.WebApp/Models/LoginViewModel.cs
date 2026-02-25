using System.ComponentModel.DataAnnotations;

namespace AppSimple.WebApp.Models;

/// <summary>View model for the login form.</summary>
public sealed class LoginViewModel
{
    /// <summary>Username to authenticate with.</summary>
    [Required]
    public string Username { get; set; } = "";

    /// <summary>Password to authenticate with.</summary>
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    /// <summary>URL to redirect to after successful login.</summary>
    public string? ReturnUrl { get; set; }

    /// <summary>Error message to display if login fails.</summary>
    public string? Error { get; set; }
}
