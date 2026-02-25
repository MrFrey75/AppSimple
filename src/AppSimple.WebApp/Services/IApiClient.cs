using AppSimple.Core.Http;
using AppSimple.Core.Models.DTOs;
using AppSimple.Core.Models.Requests;

namespace AppSimple.WebApp.Services;

/// <summary>Abstraction for communicating with the AppSimple WebApi.</summary>
public interface IApiClient : IAppApiClient
{
    /// <summary>Returns the currently authenticated user's profile.</summary>
    Task<UserDto?> GetMeAsync(string token);

    /// <summary>Updates the currently authenticated user's profile.</summary>
    Task<UserDto?> UpdateMeAsync(string token, UpdateUserRequest request);

    /// <summary>Changes the current user's password.</summary>
    Task<bool> ChangePasswordAsync(string token, string currentPassword, string newPassword);
}
