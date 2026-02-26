using AppSimple.Core.Enums;
using AppSimple.Core.Models;

namespace AppSimple.Core.Services;

/// <summary>
/// Service for managing <see cref="Contact"/> entities and their child collections.
/// </summary>
/// <remarks>
/// Access rules:
/// <list type="bullet">
///   <item>Users may only read and modify their own contacts.</item>
///   <item>Admins may read all contacts, but may only edit or delete contacts they own.</item>
/// </list>
/// </remarks>
public interface IContactService
{
    /// <summary>Gets a contact by its unique identifier, with child collections populated.</summary>
    Task<Contact?> GetByUidAsync(Guid uid);

    /// <summary>Returns all contacts in the system. Intended for admin use.</summary>
    Task<IEnumerable<Contact>> GetAllAsync();

    /// <summary>Returns all contacts owned by the specified user.</summary>
    Task<IEnumerable<Contact>> GetByOwnerUidAsync(Guid ownerUserUid);

    /// <summary>Creates a new contact owned by the specified user.</summary>
    /// <param name="ownerUserUid">The UID of the contact owner.</param>
    /// <param name="name">Display name for the contact.</param>
    /// <param name="tags">Optional string tags.</param>
    Task<Contact> CreateAsync(Guid ownerUserUid, string name, List<string>? tags = null);

    /// <summary>Updates a contact's top-level fields (name, tags).</summary>
    Task UpdateAsync(Contact contact);

    /// <summary>Deletes a contact and all its child records.</summary>
    Task DeleteAsync(Guid uid);

    // ── Email addresses ───────────────────────────────────────────────────

    /// <summary>Adds an email address to an existing contact.</summary>
    Task<EmailAddress> AddEmailAddressAsync(Guid contactUid, string email, EmailType type, bool isPrimary = false, List<string>? tags = null);

    /// <summary>Updates an existing email address.</summary>
    Task UpdateEmailAddressAsync(EmailAddress emailAddress);

    /// <summary>Deletes an email address by its UID.</summary>
    Task DeleteEmailAddressAsync(Guid emailAddressUid);

    // ── Phone numbers ─────────────────────────────────────────────────────

    /// <summary>Adds a phone number to an existing contact.</summary>
    Task<PhoneNumber> AddPhoneNumberAsync(Guid contactUid, string number, PhoneType type, bool isPrimary = false, List<string>? tags = null);

    /// <summary>Updates an existing phone number.</summary>
    Task UpdatePhoneNumberAsync(PhoneNumber phoneNumber);

    /// <summary>Deletes a phone number by its UID.</summary>
    Task DeletePhoneNumberAsync(Guid phoneNumberUid);

    // ── Postal addresses ──────────────────────────────────────────────────

    /// <summary>Adds a postal address to an existing contact.</summary>
    Task<ContactAddress> AddAddressAsync(Guid contactUid, ContactAddress address);

    /// <summary>Updates an existing postal address.</summary>
    Task UpdateAddressAsync(ContactAddress address);

    /// <summary>Deletes a postal address by its UID.</summary>
    Task DeleteAddressAsync(Guid addressUid);
}
