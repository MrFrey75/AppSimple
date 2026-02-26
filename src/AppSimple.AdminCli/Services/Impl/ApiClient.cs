using AppSimple.Core.Http.Impl;
using AppSimple.Core.Logging;
using AppSimple.Core.Models.DTOs;
using AppSimple.Core.Models.Requests;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AppSimple.AdminCli.Services.Impl;

/// <summary>
/// Typed HTTP client that communicates with the AppSimple WebApi.
/// All methods return <c>null</c> / empty list / <c>false</c> on non-success instead of throwing.
/// </summary>
public sealed class ApiClient : ApiClientBase, IApiClient
{
    private readonly IAppLogger<ApiClient> _logger;

    /// <summary>Initializes a new instance of <see cref="ApiClient"/>.</summary>
    public ApiClient(HttpClient http, IAppLogger<ApiClient> logger) : base(http)
    {
        _logger = logger;
        _logger.Debug("ApiClient initialized") ;
    }

    // ── Override base methods to add resilient try-catch ─────────────────────

    /// <inheritdoc/>
    public override async Task<LoginResult?> LoginAsync(string username, string password)
    {
        try { return await base.LoginAsync(username, password); }
        catch (Exception ex) { _logger.Error(ex, "LoginAsync failed for user '{Username}'", username); return null; }
    }

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(string token)
    {
        try { return await base.GetAllUsersAsync(token); }
        catch (Exception ex) { _logger.Error(ex, "GetAllUsersAsync failed"); return []; }
    }

    /// <inheritdoc/>
    public override async Task<UserDto?> GetUserAsync(string token, Guid uid)
    {
        try { return await base.GetUserAsync(token, uid); }
        catch (Exception ex) { _logger.Error(ex, "GetUserAsync failed for UID {Uid}", uid); return null; }
    }

    /// <inheritdoc/>
    public override async Task<UserDto?> CreateUserAsync(string token, string username, string email, string password)
    {
        try { return await base.CreateUserAsync(token, username, email, password); }
        catch (Exception ex) { _logger.Error(ex, "CreateUserAsync failed for '{Username}'", username); return null; }
    }

    /// <inheritdoc/>
    public override async Task<UserDto?> UpdateUserAsync(string token, Guid uid, UpdateUserRequest request)
    {
        try { return await base.UpdateUserAsync(token, uid, request); }
        catch (Exception ex) { _logger.Error(ex, "UpdateUserAsync failed for UID {Uid}", uid); return null; }
    }

    /// <inheritdoc/>
    public override async Task<bool> DeleteUserAsync(string token, Guid uid)
    {
        try { return await base.DeleteUserAsync(token, uid); }
        catch (Exception ex) { _logger.Error(ex, "DeleteUserAsync failed for UID {Uid}", uid); return false; }
    }

    // ── AdminCli-specific methods ─────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<HealthResult?> GetHealthAsync()
    {
        try
        {
            using var response = await _http.GetAsync("api/health");
            return await ReadAsync<HealthResult>(response);
        }
        catch (Exception ex) { _logger.Error(ex, "GetHealthAsync failed"); return null; }
    }

    /// <inheritdoc/>
    public async Task<bool> SetUserRoleAsync(string token, Guid uid, int role)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/admin/users/{uid}/role");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(JsonSerializer.Serialize(role), Encoding.UTF8, "application/json");
            using var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) { _logger.Error(ex, "SetUserRoleAsync failed for UID {Uid}", uid); return false; }
    }

    /// <inheritdoc/>
    public async Task<bool> PingProtectedAsync(string token)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/protected");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            using var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) { _logger.Error(ex, "PingProtectedAsync failed"); return false; }
    }
}
