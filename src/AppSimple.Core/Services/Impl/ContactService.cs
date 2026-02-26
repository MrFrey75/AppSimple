using AppSimple.Core.Enums;
using AppSimple.Core.Interfaces;
using AppSimple.Core.Logging;
using AppSimple.Core.Models;

namespace AppSimple.Core.Services.Impl;

/// <summary>
/// Core implementation of <see cref="IContactService"/> backed by <see cref="IContactRepository"/>.
/// </summary>
public sealed class ContactService : IContactService
{
    private readonly IContactRepository _contacts;
    private readonly IAppLogger<ContactService> _logger;

    /// <summary>Initializes a new instance of <see cref="ContactService"/>.</summary>
    public ContactService(IContactRepository contacts, IAppLogger<ContactService> logger)
    {
        _contacts = contacts;
        _logger   = logger;
        _logger.Debug("ContactService initialized.");
    }

    /// <inheritdoc />
    public Task<Contact?> GetByUidAsync(Guid uid)
    {
        _logger.Debug("GetByUid: {Uid}", uid);
        return _contacts.GetByUidAsync(uid);
    }

    /// <inheritdoc />
    public Task<IEnumerable<Contact>> GetAllAsync()
    {
        _logger.Debug("GetAll contacts requested.");
        return _contacts.GetAllAsync();
    }

    /// <inheritdoc />
    public Task<IEnumerable<Contact>> GetByOwnerUidAsync(Guid ownerUserUid)
    {
        _logger.Debug("GetByOwnerUid: {OwnerUserUid}", ownerUserUid);
        return _contacts.GetByOwnerUidAsync(ownerUserUid);
    }

    /// <inheritdoc />
    public async Task<Contact> CreateAsync(Guid ownerUserUid, string name, List<string>? tags = null)
    {
        var now = DateTime.UtcNow;
        var contact = new Contact
        {
            Uid          = Guid.CreateVersion7(),
            OwnerUserUid = ownerUserUid,
            Name         = name,
            Tags         = tags ?? [],
            CreatedAt    = now,
            UpdatedAt    = now,
        };

        await _contacts.AddAsync(contact);
        _logger.Information("Contact '{Name}' ({Uid}) created for user {OwnerUserUid}.", name, contact.Uid, ownerUserUid);
        return contact;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Contact contact)
    {
        contact.UpdatedAt = DateTime.UtcNow;
        await _contacts.UpdateAsync(contact);
        _logger.Information("Contact {Uid} updated.", contact.Uid);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid uid)
    {
        await _contacts.DeleteAsync(uid);
        _logger.Information("Contact {Uid} deleted.", uid);
    }

    // ── Email addresses ───────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<EmailAddress> AddEmailAddressAsync(
        Guid contactUid, string email, EmailType type, bool isPrimary = false, List<string>? tags = null)
    {
        var now = DateTime.UtcNow;
        var entity = new EmailAddress
        {
            Uid        = Guid.CreateVersion7(),
            ContactUid = contactUid,
            Email      = email,
            Type       = type,
            IsPrimary  = isPrimary,
            Tags       = tags ?? [],
            CreatedAt  = now,
            UpdatedAt  = now,
        };
        await _contacts.AddEmailAddressAsync(entity);
        _logger.Debug("Email '{Email}' added to contact {ContactUid}.", email, contactUid);
        return entity;
    }

    /// <inheritdoc />
    public async Task UpdateEmailAddressAsync(EmailAddress emailAddress)
    {
        emailAddress.UpdatedAt = DateTime.UtcNow;
        await _contacts.UpdateEmailAddressAsync(emailAddress);
        _logger.Debug("EmailAddress {Uid} updated.", emailAddress.Uid);
    }

    /// <inheritdoc />
    public async Task DeleteEmailAddressAsync(Guid emailAddressUid)
    {
        await _contacts.DeleteEmailAddressAsync(emailAddressUid);
        _logger.Debug("EmailAddress {Uid} deleted.", emailAddressUid);
    }

    // ── Phone numbers ─────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<PhoneNumber> AddPhoneNumberAsync(
        Guid contactUid, string number, PhoneType type, bool isPrimary = false, List<string>? tags = null)
    {
        var now = DateTime.UtcNow;
        var entity = new PhoneNumber
        {
            Uid        = Guid.CreateVersion7(),
            ContactUid = contactUid,
            Number     = number,
            Type       = type,
            IsPrimary  = isPrimary,
            Tags       = tags ?? [],
            CreatedAt  = now,
            UpdatedAt  = now,
        };
        await _contacts.AddPhoneNumberAsync(entity);
        _logger.Debug("Phone '{Number}' added to contact {ContactUid}.", number, contactUid);
        return entity;
    }

    /// <inheritdoc />
    public async Task UpdatePhoneNumberAsync(PhoneNumber phoneNumber)
    {
        phoneNumber.UpdatedAt = DateTime.UtcNow;
        await _contacts.UpdatePhoneNumberAsync(phoneNumber);
        _logger.Debug("PhoneNumber {Uid} updated.", phoneNumber.Uid);
    }

    /// <inheritdoc />
    public async Task DeletePhoneNumberAsync(Guid phoneNumberUid)
    {
        await _contacts.DeletePhoneNumberAsync(phoneNumberUid);
        _logger.Debug("PhoneNumber {Uid} deleted.", phoneNumberUid);
    }

    // ── Postal addresses ──────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<ContactAddress> AddAddressAsync(Guid contactUid, ContactAddress address)
    {
        var now = DateTime.UtcNow;
        address.Uid        = Guid.CreateVersion7();
        address.ContactUid = contactUid;
        address.CreatedAt  = now;
        address.UpdatedAt  = now;
        await _contacts.AddAddressAsync(address);
        _logger.Debug("Address added to contact {ContactUid}.", contactUid);
        return address;
    }

    /// <inheritdoc />
    public async Task UpdateAddressAsync(ContactAddress address)
    {
        address.UpdatedAt = DateTime.UtcNow;
        await _contacts.UpdateAddressAsync(address);
        _logger.Debug("Address {Uid} updated.", address.Uid);
    }

    /// <inheritdoc />
    public async Task DeleteAddressAsync(Guid addressUid)
    {
        await _contacts.DeleteAddressAsync(addressUid);
        _logger.Debug("Address {Uid} deleted.", addressUid);
    }
}
