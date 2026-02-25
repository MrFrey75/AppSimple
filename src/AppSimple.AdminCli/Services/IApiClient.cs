using AppSimple.Core.Http;
using AppSimple.Core.Models.DTOs;
using AppSimple.Core.Models.Requests;

namespace AppSimple.AdminCli.Services;

/// <summary>Typed HTTP client for communicating with the AppSimple WebApi.</summary>
public interface IApiClient : IAppApiClient
{
    /// <summary>Retrieves the API health status.</summary>
    Task<HealthResult?> GetHealthAsync();

    /// <summary>Sets the role for a user. Returns <c>true</c> on success.</summary>
    Task<bool> SetUserRoleAsync(string token, Guid uid, int role);

    /// <summary>Pings the protected endpoint to verify authentication works. Returns <c>true</c> on 200.</summary>
    Task<bool> PingProtectedAsync(string token);
}
