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

    // ── Profile ──────────────────────────────────────────────────────────────

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

    // ── Notes ────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NoteDto>> GetMyNotesAsync(string token)
    {
        SetBearer(token);
        using var response = await _http.GetAsync("/api/notes");
        return await ReadAsync<List<NoteDto>>(response) ?? [];
    }

    /// <inheritdoc/>
    public async Task<NoteDto?> GetNoteAsync(string token, Guid uid)
    {
        SetBearer(token);
        using var response = await _http.GetAsync($"/api/notes/{uid}");
        return await ReadAsync<NoteDto>(response);
    }

    /// <inheritdoc/>
    public async Task<NoteDto?> CreateNoteAsync(string token, CreateNoteRequest request)
    {
        SetBearer(token);
        using var response = await _http.PostAsync("/api/notes", ToJson(request));
        return await ReadAsync<NoteDto>(response);
    }

    /// <inheritdoc/>
    public async Task<NoteDto?> UpdateNoteAsync(string token, Guid uid, UpdateNoteRequest request)
    {
        SetBearer(token);
        using var response = await _http.PutAsync($"/api/notes/{uid}", ToJson(request));
        return await ReadAsync<NoteDto>(response);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteNoteAsync(string token, Guid uid)
    {
        SetBearer(token);
        using var response = await _http.DeleteAsync($"/api/notes/{uid}");
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc/>
    public async Task<bool> AddTagToNoteAsync(string token, Guid noteUid, Guid tagUid)
    {
        SetBearer(token);
        using var response = await _http.PostAsync($"/api/notes/{noteUid}/tags/{tagUid}", null);
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveTagFromNoteAsync(string token, Guid noteUid, Guid tagUid)
    {
        SetBearer(token);
        using var response = await _http.DeleteAsync($"/api/notes/{noteUid}/tags/{tagUid}");
        return response.IsSuccessStatusCode;
    }

    // ── Tags ─────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TagDto>> GetMyTagsAsync(string token)
    {
        SetBearer(token);
        using var response = await _http.GetAsync("/api/tags");
        return await ReadAsync<List<TagDto>>(response) ?? [];
    }

    /// <inheritdoc/>
    public async Task<TagDto?> GetTagAsync(string token, Guid uid)
    {
        SetBearer(token);
        using var response = await _http.GetAsync($"/api/tags/{uid}");
        return await ReadAsync<TagDto>(response);
    }

    /// <inheritdoc/>
    public async Task<TagDto?> CreateTagAsync(string token, CreateTagRequest request)
    {
        SetBearer(token);
        using var response = await _http.PostAsync("/api/tags", ToJson(request));
        return await ReadAsync<TagDto>(response);
    }

    /// <inheritdoc/>
    public async Task<TagDto?> UpdateTagAsync(string token, Guid uid, UpdateTagRequest request)
    {
        SetBearer(token);
        using var response = await _http.PutAsync($"/api/tags/{uid}", ToJson(request));
        return await ReadAsync<TagDto>(response);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteTagAsync(string token, Guid uid)
    {
        SetBearer(token);
        using var response = await _http.DeleteAsync($"/api/tags/{uid}");
        return response.IsSuccessStatusCode;
    }

    // ── Contacts ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ContactDto>> GetMyContactsAsync(string token)
    {
        SetBearer(token);
        using var response = await _http.GetAsync("/api/contacts");
        return await ReadAsync<List<ContactDto>>(response) ?? [];
    }

    /// <inheritdoc/>
    public async Task<ContactDto?> GetContactAsync(string token, Guid uid)
    {
        SetBearer(token);
        using var response = await _http.GetAsync($"/api/contacts/{uid}");
        return await ReadAsync<ContactDto>(response);
    }

    /// <inheritdoc/>
    public async Task<ContactDto?> CreateContactAsync(string token, CreateContactRequest request)
    {
        SetBearer(token);
        using var response = await _http.PostAsync("/api/contacts", ToJson(request));
        return await ReadAsync<ContactDto>(response);
    }

    /// <inheritdoc/>
    public async Task<ContactDto?> UpdateContactAsync(string token, Guid uid, UpdateContactRequest request)
    {
        SetBearer(token);
        using var response = await _http.PutAsync($"/api/contacts/{uid}", ToJson(request));
        return await ReadAsync<ContactDto>(response);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteContactAsync(string token, Guid uid)
    {
        SetBearer(token);
        using var response = await _http.DeleteAsync($"/api/contacts/{uid}");
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc/>
    public async Task<EmailAddressDto?> AddEmailAsync(string token, Guid contactUid, ContactEmailRequest request)
    {
        SetBearer(token);
        using var response = await _http.PostAsync($"/api/contacts/{contactUid}/emails", ToJson(request));
        return await ReadAsync<EmailAddressDto>(response);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateEmailAsync(string token, Guid emailUid, ContactEmailRequest request)
    {
        SetBearer(token);
        using var response = await _http.PutAsync($"/api/contacts/emails/{emailUid}", ToJson(request));
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteEmailAsync(string token, Guid emailUid)
    {
        SetBearer(token);
        using var response = await _http.DeleteAsync($"/api/contacts/emails/{emailUid}");
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc/>
    public async Task<PhoneNumberDto?> AddPhoneAsync(string token, Guid contactUid, ContactPhoneRequest request)
    {
        SetBearer(token);
        using var response = await _http.PostAsync($"/api/contacts/{contactUid}/phones", ToJson(request));
        return await ReadAsync<PhoneNumberDto>(response);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdatePhoneAsync(string token, Guid phoneUid, ContactPhoneRequest request)
    {
        SetBearer(token);
        using var response = await _http.PutAsync($"/api/contacts/phones/{phoneUid}", ToJson(request));
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc/>
    public async Task<bool> DeletePhoneAsync(string token, Guid phoneUid)
    {
        SetBearer(token);
        using var response = await _http.DeleteAsync($"/api/contacts/phones/{phoneUid}");
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc/>
    public async Task<ContactAddressDto?> AddAddressAsync(string token, Guid contactUid, ContactAddressRequest request)
    {
        SetBearer(token);
        using var response = await _http.PostAsync($"/api/contacts/{contactUid}/addresses", ToJson(request));
        return await ReadAsync<ContactAddressDto>(response);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAddressAsync(string token, Guid addressUid, ContactAddressRequest request)
    {
        SetBearer(token);
        using var response = await _http.PutAsync($"/api/contacts/addresses/{addressUid}", ToJson(request));
        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAddressAsync(string token, Guid addressUid)
    {
        SetBearer(token);
        using var response = await _http.DeleteAsync($"/api/contacts/addresses/{addressUid}");
        return response.IsSuccessStatusCode;
    }
}
