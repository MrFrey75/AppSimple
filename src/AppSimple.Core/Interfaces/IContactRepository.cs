using AppSimple.Core.Models;

namespace AppSimple.Core.Interfaces;

/// <summary>
/// Repository interface for <see cref="Contact"/> entities and their child collections
/// (email addresses, phone numbers, postal addresses).
/// </summary>
public interface IContactRepository : IRepository<Contact>
{
    /// <summary>Returns all contacts owned by the specified user, with child collections populated.</summary>
    /// <param name="ownerUserUid">The UID of the owning user.</param>
    Task<IEnumerable<Contact>> GetByOwnerUidAsync(Guid ownerUserUid);

    // ── Email addresses ───────────────────────────────────────────────────

    /// <summary>Adds an email address to a contact.</summary>
    Task AddEmailAddressAsync(EmailAddress email);

    /// <summary>Updates an existing email address.</summary>
    Task UpdateEmailAddressAsync(EmailAddress email);

    /// <summary>Deletes an email address by its UID.</summary>
    Task DeleteEmailAddressAsync(Guid uid);

    // ── Phone numbers ─────────────────────────────────────────────────────

    /// <summary>Adds a phone number to a contact.</summary>
    Task AddPhoneNumberAsync(PhoneNumber phone);

    /// <summary>Updates an existing phone number.</summary>
    Task UpdatePhoneNumberAsync(PhoneNumber phone);

    /// <summary>Deletes a phone number by its UID.</summary>
    Task DeletePhoneNumberAsync(Guid uid);

    // ── Postal addresses ──────────────────────────────────────────────────

    /// <summary>Adds a postal address to a contact.</summary>
    Task AddAddressAsync(ContactAddress address);

    /// <summary>Updates an existing postal address.</summary>
    Task UpdateAddressAsync(ContactAddress address);

    /// <summary>Deletes a postal address by its UID.</summary>
    Task DeleteAddressAsync(Guid uid);
}
