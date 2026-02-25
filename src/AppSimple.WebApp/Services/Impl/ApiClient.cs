using AppSimple.Core.Http.Impl;
using AppSimple.Core.Models.DTOs;
using AppSimple.Core.Models.Requests;
using Microsoft.Extensions.Logging;

namespace AppSimple.WebApp.Services.Impl;

/// <summary>HTTP client implementation for communicating with the AppSimple WebApi.</summary>
public sealed class ApiClient : ApiClientBase, IApiClient
{
    private readonly ILogger<ApiClient> _logger;

    /// <summary>Initializes a new instance of <see cref="ApiClient"/>.</summary>
    public ApiClient(HttpClient http, ILogger<ApiClient> logger) : base(http)
    {
        _logger = logger;
        _logger.LogInformation("ApiClient initialized with base address {BaseAddress}", http.BaseAddress);
    }

    /// <inheritdoc/>
    protected override async Task<T?> ReadAsync<T>(HttpResponseMessage response) where T : class
    {
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("API returned {Status} for {Url}", (int)response.StatusCode, response.RequestMessage?.RequestUri);
            return default;
        }
        return await base.ReadAsync<T>(response);
    }

    /// <inheritdoc/>
    public async Task<UserDto?> GetMeAsync(string token)
    {
        SetBearer(token);
        using var response = await _http.GetAsync("/api/protected/me");
        return await ReadAsync<UserDto>(response);
    }

    /// <inheritdoc/>
    public async Task<UserDto?> UpdateMeAsync(string token, UpdateUserRequest request)
    {
        SetBearer(token);
        using var response = await _http.PutAsync("/api/protected/me", ToJson(request));
        return await ReadAsync<UserDto>(response);
    }

    /// <inheritdoc/>
    public async Task<bool> ChangePasswordAsync(string token, string currentPassword, string newPassword)
    {
        SetBearer(token);
        using var response = await _http.PostAsync("/api/protected/me/change-password", ToJson(new { currentPassword, newPassword }));
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("ChangePassword returned {Status}", (int)response.StatusCode);
            return false;
        }
        return true;
    }
}
