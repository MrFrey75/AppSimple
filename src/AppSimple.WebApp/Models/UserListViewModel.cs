using AppSimple.WebApp.Services;

namespace AppSimple.WebApp.Models;

/// <summary>View model for the admin user list page.</summary>
public sealed class UserListViewModel
{
    public IReadOnlyList<UserDto> Users { get; set; } = [];
    public string? Error { get; set; }
}
