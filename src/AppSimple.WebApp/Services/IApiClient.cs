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

    // ── Notes ───────────────────────────────────────────────────────────────

    /// <summary>Returns all notes belonging to the authenticated user.</summary>
    Task<IReadOnlyList<NoteDto>> GetMyNotesAsync(string token);

    /// <summary>Returns a single note by UID.</summary>
    Task<NoteDto?> GetNoteAsync(string token, Guid uid);

    /// <summary>Creates a new note.</summary>
    Task<NoteDto?> CreateNoteAsync(string token, CreateNoteRequest request);

    /// <summary>Updates an existing note.</summary>
    Task<NoteDto?> UpdateNoteAsync(string token, Guid uid, UpdateNoteRequest request);

    /// <summary>Deletes a note.</summary>
    Task<bool> DeleteNoteAsync(string token, Guid uid);

    /// <summary>Adds a tag to a note.</summary>
    Task<bool> AddTagToNoteAsync(string token, Guid noteUid, Guid tagUid);

    /// <summary>Removes a tag from a note.</summary>
    Task<bool> RemoveTagFromNoteAsync(string token, Guid noteUid, Guid tagUid);

    // ── Tags ────────────────────────────────────────────────────────────────

    /// <summary>Returns all tags belonging to the authenticated user.</summary>
    Task<IReadOnlyList<TagDto>> GetMyTagsAsync(string token);

    /// <summary>Returns a single tag by UID.</summary>
    Task<TagDto?> GetTagAsync(string token, Guid uid);

    /// <summary>Creates a new tag.</summary>
    Task<TagDto?> CreateTagAsync(string token, CreateTagRequest request);

    /// <summary>Updates an existing tag.</summary>
    Task<TagDto?> UpdateTagAsync(string token, Guid uid, UpdateTagRequest request);

    /// <summary>Deletes a tag.</summary>
    Task<bool> DeleteTagAsync(string token, Guid uid);

    // ── Contacts ────────────────────────────────────────────────────────────

    /// <summary>Returns all contacts owned by the authenticated user.</summary>
    Task<IReadOnlyList<ContactDto>> GetMyContactsAsync(string token);

    /// <summary>Returns a single contact by UID with child collections.</summary>
    Task<ContactDto?> GetContactAsync(string token, Guid uid);

    /// <summary>Creates a new contact.</summary>
    Task<ContactDto?> CreateContactAsync(string token, CreateContactRequest request);

    /// <summary>Updates a contact's top-level fields.</summary>
    Task<ContactDto?> UpdateContactAsync(string token, Guid uid, UpdateContactRequest request);

    /// <summary>Deletes a contact and all its child records.</summary>
    Task<bool> DeleteContactAsync(string token, Guid uid);

    /// <summary>Adds an email address to a contact.</summary>
    Task<EmailAddressDto?> AddEmailAsync(string token, Guid contactUid, ContactEmailRequest request);

    /// <summary>Updates an email address.</summary>
    Task<bool> UpdateEmailAsync(string token, Guid emailUid, ContactEmailRequest request);

    /// <summary>Deletes an email address.</summary>
    Task<bool> DeleteEmailAsync(string token, Guid emailUid);

    /// <summary>Adds a phone number to a contact.</summary>
    Task<PhoneNumberDto?> AddPhoneAsync(string token, Guid contactUid, ContactPhoneRequest request);

    /// <summary>Updates a phone number.</summary>
    Task<bool> UpdatePhoneAsync(string token, Guid phoneUid, ContactPhoneRequest request);

    /// <summary>Deletes a phone number.</summary>
    Task<bool> DeletePhoneAsync(string token, Guid phoneUid);

    /// <summary>Adds a postal address to a contact.</summary>
    Task<ContactAddressDto?> AddAddressAsync(string token, Guid contactUid, ContactAddressRequest request);

    /// <summary>Updates a postal address.</summary>
    Task<bool> UpdateAddressAsync(string token, Guid addressUid, ContactAddressRequest request);

    /// <summary>Deletes a postal address.</summary>
    Task<bool> DeleteAddressAsync(string token, Guid addressUid);
}
