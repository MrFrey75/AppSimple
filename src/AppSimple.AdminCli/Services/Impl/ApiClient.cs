using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AppSimple.AdminCli.Services.Impl;

/// <summary>
/// Typed HTTP client that communicates with the AppSimple WebApi.
/// All methods return <c>null</c> / empty list / <c>false</c> on non-success instead of throwing.
/// </summary>
public sealed class ApiClient : IApiClient
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Initializes a new instance of <see cref="ApiClient"/>.</summary>
    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    /// <inheritdoc/>
    public async Task<LoginResult?> LoginAsync(string username, string password)
    {
        try
        {
            var body = new { username, password };
            using var response = await _http.PostAsJsonAsync("api/auth/login", body);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<LoginResult>(_jsonOpts);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<HealthResult?> GetHealthAsync()
    {
        try
        {
            using var response = await _http.GetAsync("api/health");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<HealthResult>(_jsonOpts);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(string token)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/admin/users");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return [];
            return await response.Content.ReadFromJsonAsync<List<UserDto>>(_jsonOpts) ?? [];
        }
        catch
        {
            return [];
        }
    }

    /// <inheritdoc/>
    public async Task<UserDto?> GetUserAsync(string token, Guid uid)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/admin/users/{uid}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserDto>(_jsonOpts);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<UserDto?> CreateUserAsync(string token, string username, string email, string password)
    {
        try
        {
            var body = new { username, email, password };
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/admin/users");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(
                JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            using var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserDto>(_jsonOpts);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<UserDto?> UpdateUserAsync(string token, Guid uid, UpdateUserRequest req)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"api/admin/users/{uid}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(
                JsonSerializer.Serialize(req), Encoding.UTF8, "application/json");
            using var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserDto>(_jsonOpts);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteUserAsync(string token, Guid uid)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/admin/users/{uid}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SetUserRoleAsync(string token, Guid uid, int role)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/admin/users/{uid}/role");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(
                JsonSerializer.Serialize(role), Encoding.UTF8, "application/json");
            using var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> PingProtectedAsync(string token)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/protected");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
