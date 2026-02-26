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
    }

    /// <inheritdoc />
    public async Task<Contact?> GetByUidAsync(Guid uid)
    {
        try
        {
            return await _contacts.GetByUidAsync(uid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving contact {Uid}.", uid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Contact>> GetAllAsync()
    {
        try
        {
            return await _contacts.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving all contacts.");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Contact>> GetByOwnerUidAsync(Guid ownerUserUid)
    {
        try
        {
            return await _contacts.GetByOwnerUidAsync(ownerUserUid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving contacts for user {OwnerUserUid}.", ownerUserUid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Contact> CreateAsync(Guid ownerUserUid, string name, List<string>? tags = null)
    {
        try
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
        catch (Exception ex)
        {
            _logger.Error(ex, "Error creating contact '{Name}' for user {OwnerUserUid}.", name, ownerUserUid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Contact contact)
    {
        try
        {
            contact.UpdatedAt = DateTime.UtcNow;
            await _contacts.UpdateAsync(contact);
            _logger.Information("Contact {Uid} updated.", contact.Uid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error updating contact {Uid}.", contact.Uid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid uid)
    {
        try
        {
            await _contacts.DeleteAsync(uid);
            _logger.Information("Contact {Uid} deleted.", uid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error deleting contact {Uid}.", uid);
            throw;
        }
    }

    // ── Email addresses ───────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<EmailAddress> AddEmailAddressAsync(
        Guid contactUid, string email, EmailType type, bool isPrimary = false, List<string>? tags = null)
    {
        try
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
        catch (Exception ex)
        {
            _logger.Error(ex, "Error adding email to contact {ContactUid}.", contactUid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task UpdateEmailAddressAsync(EmailAddress emailAddress)
    {
        try
        {
            emailAddress.UpdatedAt = DateTime.UtcNow;
            await _contacts.UpdateEmailAddressAsync(emailAddress);
            _logger.Debug("EmailAddress {Uid} updated.", emailAddress.Uid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error updating email address {Uid}.", emailAddress.Uid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteEmailAddressAsync(Guid emailAddressUid)
    {
        try
        {
            await _contacts.DeleteEmailAddressAsync(emailAddressUid);
            _logger.Debug("EmailAddress {Uid} deleted.", emailAddressUid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error deleting email address {Uid}.", emailAddressUid);
            throw;
        }
    }

    // ── Phone numbers ─────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<PhoneNumber> AddPhoneNumberAsync(
        Guid contactUid, string number, PhoneType type, bool isPrimary = false, List<string>? tags = null)
    {
        try
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
        catch (Exception ex)
        {
            _logger.Error(ex, "Error adding phone to contact {ContactUid}.", contactUid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task UpdatePhoneNumberAsync(PhoneNumber phoneNumber)
    {
        try
        {
            phoneNumber.UpdatedAt = DateTime.UtcNow;
            await _contacts.UpdatePhoneNumberAsync(phoneNumber);
            _logger.Debug("PhoneNumber {Uid} updated.", phoneNumber.Uid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error updating phone number {Uid}.", phoneNumber.Uid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeletePhoneNumberAsync(Guid phoneNumberUid)
    {
        try
        {
            await _contacts.DeletePhoneNumberAsync(phoneNumberUid);
            _logger.Debug("PhoneNumber {Uid} deleted.", phoneNumberUid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error deleting phone number {Uid}.", phoneNumberUid);
            throw;
        }
    }

    // ── Postal addresses ──────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<ContactAddress> AddAddressAsync(Guid contactUid, ContactAddress address)
    {
        try
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
        catch (Exception ex)
        {
            _logger.Error(ex, "Error adding address to contact {ContactUid}.", contactUid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task UpdateAddressAsync(ContactAddress address)
    {
        try
        {
            address.UpdatedAt = DateTime.UtcNow;
            await _contacts.UpdateAddressAsync(address);
            _logger.Debug("Address {Uid} updated.", address.Uid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error updating address {Uid}.", address.Uid);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteAddressAsync(Guid addressUid)
    {
        try
        {
            await _contacts.DeleteAddressAsync(addressUid);
            _logger.Debug("Address {Uid} deleted.", addressUid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error deleting address {Uid}.", addressUid);
            throw;
        }
    }
}
