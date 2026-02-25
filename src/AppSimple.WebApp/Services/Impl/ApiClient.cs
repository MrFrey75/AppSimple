using AppSimple.Core.Models.DTOs;
using AppSimple.Core.Models.Requests;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AppSimple.WebApp.Services.Impl;

/// <summary>HTTP client implementation for communicating with the AppSimple WebApi.</summary>
public sealed class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiClient> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Initializes a new instance of <see cref="ApiClient"/>.</summary>
    public ApiClient(HttpClient http, ILogger<ApiClient> logger)
    {
        _http = http;
        _logger = logger;
        _logger.LogInformation("ApiClient initialized with base address {BaseAddress}", http.BaseAddress);
    }

    private static void SetBearer(HttpClient http, string token)
    {
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static StringContent ToJson(object obj) =>
        new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    private async Task<T?> ReadAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("API returned {Status} for {Url}", (int)response.StatusCode, response.RequestMessage?.RequestUri);
            return default;
        }
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    /// <inheritdoc/>
    public async Task<LoginResult?> LoginAsync(string username, string password)
    {
        var body = ToJson(new { username, password });
        var response = await _http.PostAsync("/api/auth/login", body);
        return await ReadAsync<LoginResult>(response);
    }

    /// <inheritdoc/>
    public async Task<UserDto?> GetMeAsync(string token)
    {
        SetBearer(_http, token);
        var response = await _http.GetAsync("/api/protected/me");
        return await ReadAsync<UserDto>(response);
    }

    /// <inheritdoc/>
    public async Task<UserDto?> UpdateMeAsync(string token, UpdateUserRequest request)
    {
        SetBearer(_http, token);
        var response = await _http.PutAsync("/api/protected/me", ToJson(request));
        return await ReadAsync<UserDto>(response);
    }

    /// <inheritdoc/>
    public async Task<bool> ChangePasswordAsync(string token, string currentPassword, string newPassword)
    {
        SetBearer(_http, token);
        var body = ToJson(new { currentPassword, newPassword });
        var response = await _http.PostAsync("/api/protected/me/change-password", body);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("ChangePassword returned {Status}", (int)response.StatusCode);
            return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(string token)
    {
        SetBearer(_http, token);
        var response = await _http.GetAsync("/api/admin/users");
        return await ReadAsync<List<UserDto>>(response) ?? [];
    }

    /// <inheritdoc/>
    public async Task<UserDto?> GetUserAsync(string token, Guid uid)
    {
        SetBearer(_http, token);
        var response = await _http.GetAsync($"/api/admin/users/{uid}");
        return await ReadAsync<UserDto>(response);
    }

    /// <inheritdoc/>
    public async Task<UserDto?> CreateUserAsync(string token, string username, string email, string password)
    {
        SetBearer(_http, token);
        var body = ToJson(new { username, email, password });
        var response = await _http.PostAsync("/api/admin/users", body);
        return await ReadAsync<UserDto>(response);
    }

    /// <inheritdoc/>
    public async Task<UserDto?> UpdateUserAsync(string token, Guid uid, UpdateUserRequest request)
    {
        SetBearer(_http, token);
        var response = await _http.PutAsync($"/api/admin/users/{uid}", ToJson(request));
        return await ReadAsync<UserDto>(response);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteUserAsync(string token, Guid uid)
    {
        SetBearer(_http, token);
        var response = await _http.DeleteAsync($"/api/admin/users/{uid}");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("DeleteUser returned {Status}", (int)response.StatusCode);
            return false;
        }
        return true;
    }
}
