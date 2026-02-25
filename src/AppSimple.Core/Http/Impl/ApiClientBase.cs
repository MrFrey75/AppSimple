using AppSimple.Core.Models.DTOs;
using AppSimple.Core.Models.Requests;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AppSimple.Core.Http.Impl;

/// <summary>
/// Base implementation of <see cref="IAppApiClient"/> that provides shared HTTP utilities
/// and all common WebApi operations. Project-specific clients should inherit this class
/// and add their own methods.
/// </summary>
public abstract class ApiClientBase : IAppApiClient
{
    /// <summary>The underlying HTTP client.</summary>
    protected readonly HttpClient _http;

    /// <summary>Shared JSON options for all serialization/deserialization.</summary>
    protected static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Initializes a new instance of <see cref="ApiClientBase"/>.</summary>
    protected ApiClientBase(HttpClient http)
    {
        _http = http;
    }

    /// <summary>Sets the Authorization Bearer header on the shared <see cref="_http"/> client.</summary>
    protected void SetBearer(string token)
    {
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>Serializes <paramref name="obj"/> to a JSON <see cref="StringContent"/>.</summary>
    protected static StringContent ToJson(object obj) =>
        new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    /// <summary>
    /// Reads and deserializes the response body as <typeparamref name="T"/>.
    /// Returns <c>default</c> if the response is not successful.
    /// </summary>
    protected virtual async Task<T?> ReadAsync<T>(HttpResponseMessage response) where T : class
    {
        if (!response.IsSuccessStatusCode) return default;
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
    }

    // ── IAppApiClient ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public virtual async Task<LoginResult?> LoginAsync(string username, string password)
    {
        var body = ToJson(new { username, password });
        using var response = await _http.PostAsync("api/auth/login", body);
        return await ReadAsync<LoginResult>(response);
    }

    /// <inheritdoc/>
    public virtual async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(string token)
    {
        SetBearer(token);
        using var response = await _http.GetAsync("api/admin/users");
        return await ReadAsync<List<UserDto>>(response) ?? [];
    }

    /// <inheritdoc/>
    public virtual async Task<UserDto?> GetUserAsync(string token, Guid uid)
    {
        SetBearer(token);
        using var response = await _http.GetAsync($"api/admin/users/{uid}");
        return await ReadAsync<UserDto>(response);
    }

    /// <inheritdoc/>
    public virtual async Task<UserDto?> CreateUserAsync(string token, string username, string email, string password)
    {
        SetBearer(token);
        var body = ToJson(new { username, email, password });
        using var response = await _http.PostAsync("api/admin/users", body);
        return await ReadAsync<UserDto>(response);
    }

    /// <inheritdoc/>
    public virtual async Task<UserDto?> UpdateUserAsync(string token, Guid uid, UpdateUserRequest request)
    {
        SetBearer(token);
        using var response = await _http.PutAsync($"api/admin/users/{uid}", ToJson(request));
        return await ReadAsync<UserDto>(response);
    }

    /// <inheritdoc/>
    public virtual async Task<bool> DeleteUserAsync(string token, Guid uid)
    {
        SetBearer(token);
        using var response = await _http.DeleteAsync($"api/admin/users/{uid}");
        return response.IsSuccessStatusCode;
    }
}
