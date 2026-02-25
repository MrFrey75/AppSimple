using AppSimple.Core.Models.Requests;

namespace AppSimple.WebApp.Services;

/// <summary>Abstraction for communicating with the AppSimple WebApi.</summary>
public interface IApiClient
{
    /// <summary>Authenticates a user and returns a login result with a JWT token.</summary>
    Task<LoginResult?> LoginAsync(string username, string password);

    /// <summary>Returns the currently authenticated user's profile.</summary>
    Task<UserDto?> GetMeAsync(string token);

    /// <summary>Updates the currently authenticated user's profile.</summary>
    Task<UserDto?> UpdateMeAsync(string token, UpdateUserRequest request);

    /// <summary>Changes the current user's password.</summary>
    Task<bool> ChangePasswordAsync(string token, string currentPassword, string newPassword);

    /// <summary>Returns all users (Admin only).</summary>
    Task<IReadOnlyList<UserDto>> GetAllUsersAsync(string token);

    /// <summary>Returns a specific user by UID (Admin only).</summary>
    Task<UserDto?> GetUserAsync(string token, Guid uid);

    /// <summary>Creates a new user (Admin only).</summary>
    Task<UserDto?> CreateUserAsync(string token, string username, string email, string password);

    /// <summary>Updates a specific user (Admin only).</summary>
    Task<UserDto?> UpdateUserAsync(string token, Guid uid, UpdateUserRequest request);

    /// <summary>Deletes a specific user (Admin only).</summary>
    Task<bool> DeleteUserAsync(string token, Guid uid);
}
