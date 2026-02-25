using AppSimple.Core.Models.Requests;

namespace AppSimple.AdminCli.Services;

/// <summary>Typed HTTP client for communicating with the AppSimple WebApi.</summary>
public interface IApiClient
{
    /// <summary>Attempts to log in and returns a <see cref="LoginResult"/>, or <c>null</c> on failure.</summary>
    Task<LoginResult?> LoginAsync(string username, string password);

    /// <summary>Retrieves the API health status.</summary>
    Task<HealthResult?> GetHealthAsync();

    /// <summary>Returns all users. Requires admin token.</summary>
    Task<IReadOnlyList<UserDto>> GetAllUsersAsync(string token);

    /// <summary>Returns a single user by UID, or <c>null</c> if not found.</summary>
    Task<UserDto?> GetUserAsync(string token, Guid uid);

    /// <summary>Creates a new user. Returns the created <see cref="UserDto"/>, or <c>null</c> on failure.</summary>
    Task<UserDto?> CreateUserAsync(string token, string username, string email, string password);

    /// <summary>Updates an existing user. Returns the updated <see cref="UserDto"/>, or <c>null</c> on failure.</summary>
    Task<UserDto?> UpdateUserAsync(string token, Guid uid, UpdateUserRequest request);

    /// <summary>Deletes a user by UID. Returns <c>true</c> on success.</summary>
    Task<bool> DeleteUserAsync(string token, Guid uid);

    /// <summary>Sets the role for a user. Returns <c>true</c> on success.</summary>
    Task<bool> SetUserRoleAsync(string token, Guid uid, int role);

    /// <summary>Pings the protected endpoint to verify authentication works. Returns <c>true</c> on 200.</summary>
    Task<bool> PingProtectedAsync(string token);
}
