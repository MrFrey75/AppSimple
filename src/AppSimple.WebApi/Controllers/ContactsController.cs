using AppSimple.Core.Enums;
using AppSimple.Core.Logging;
using AppSimple.Core.Models;
using AppSimple.Core.Models.DTOs;
using AppSimple.Core.Models.Requests;
using AppSimple.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppSimple.WebApi.Controllers;

/// <summary>REST endpoints for contact management including child collections.</summary>
[ApiController]
[Authorize]
public sealed class ContactsController : ControllerBase
{
    private readonly IContactService _contacts;
    private readonly IUserService _users;
    private readonly IAppLogger<ContactsController> _logger;

    /// <summary>Initializes a new instance of <see cref="ContactsController"/>.</summary>
    public ContactsController(IContactService contacts, IUserService users, IAppLogger<ContactsController> logger)
    {
        _contacts = contacts;
        _users    = users;
        _logger   = logger;
    }

    private async Task<Guid?> GetUserUidAsync()
    {
        var username = User.Identity?.Name;
        if (username is null) return null;
        var user = await _users.GetByUsernameAsync(username);
        return user?.Uid;
    }

    // ── Contacts ───────────────────────────────────────────────────────────

    /// <summary>Returns all contacts owned by the authenticated user.</summary>
    [HttpGet("api/contacts")]
    [ProducesResponseType(typeof(IEnumerable<ContactDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContacts()
    {
        var userUid = await GetUserUidAsync();
        if (userUid is null) return Unauthorized();

        var contacts = await _contacts.GetByOwnerUidAsync(userUid.Value);
        return Ok(contacts.Select(ContactDto.From));
    }

    /// <summary>Returns a single contact by UID, with child collections populated.</summary>
    [HttpGet("api/contacts/{uid:guid}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContact(Guid uid)
    {
        var contact = await _contacts.GetByUidAsync(uid);
        if (contact is null) return NotFound();
        return Ok(ContactDto.From(contact));
    }

    /// <summary>Creates a new contact owned by the authenticated user.</summary>
    [HttpPost("api/contacts")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateContact([FromBody] CreateContactRequest request)
    {
        var userUid = await GetUserUidAsync();
        if (userUid is null) return Unauthorized();

        var contact = await _contacts.CreateAsync(userUid.Value, request.Name, request.Tags);
        _logger.Information("User {User} created contact {Uid}", User.Identity?.Name, contact.Uid);
        return CreatedAtAction(nameof(GetContact), new { uid = contact.Uid }, ContactDto.From(contact));
    }

    /// <summary>Updates a contact's top-level fields.</summary>
    [HttpPut("api/contacts/{uid:guid}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateContact(Guid uid, [FromBody] UpdateContactRequest request)
    {
        var contact = await _contacts.GetByUidAsync(uid);
        if (contact is null) return NotFound();

        if (request.Name is not null) contact.Name = request.Name;
        if (request.Tags is not null) contact.Tags = request.Tags;

        await _contacts.UpdateAsync(contact);
        var updated = await _contacts.GetByUidAsync(uid);
        return Ok(ContactDto.From(updated!));
    }

    /// <summary>Deletes a contact and all its child records.</summary>
    [HttpDelete("api/contacts/{uid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContact(Guid uid)
    {
        var contact = await _contacts.GetByUidAsync(uid);
        if (contact is null) return NotFound();

        await _contacts.DeleteAsync(uid);
        _logger.Information("User {User} deleted contact {Uid}", User.Identity?.Name, uid);
        return NoContent();
    }

    // ── Email addresses ────────────────────────────────────────────────────

    /// <summary>Adds an email address to a contact.</summary>
    [HttpPost("api/contacts/{contactUid:guid}/emails")]
    [ProducesResponseType(typeof(EmailAddressDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddEmail(Guid contactUid, [FromBody] ContactEmailRequest request)
    {
        var contact = await _contacts.GetByUidAsync(contactUid);
        if (contact is null) return NotFound();

        var email = await _contacts.AddEmailAddressAsync(contactUid, request.Email, request.Type, request.IsPrimary, request.Tags);
        return StatusCode(StatusCodes.Status201Created, EmailAddressDto.From(email));
    }

    /// <summary>Updates an existing email address.</summary>
    [HttpPut("api/contacts/emails/{uid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEmail(Guid uid, [FromBody] ContactEmailRequest request)
    {
        var contact = await _contacts.GetByUidAsync(Guid.Empty); // will fetch via child
        // Load the parent contact to get the email entity
        var contacts = await _contacts.GetAllAsync();
        var email = contacts.SelectMany(c => c.EmailAddresses).FirstOrDefault(e => e.Uid == uid);
        if (email is null) return NotFound();

        email.Email     = request.Email;
        email.Type      = request.Type;
        email.IsPrimary = request.IsPrimary;
        email.Tags      = request.Tags;

        await _contacts.UpdateEmailAddressAsync(email);
        return NoContent();
    }

    /// <summary>Deletes an email address by UID.</summary>
    [HttpDelete("api/contacts/emails/{uid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteEmail(Guid uid)
    {
        await _contacts.DeleteEmailAddressAsync(uid);
        return NoContent();
    }

    // ── Phone numbers ──────────────────────────────────────────────────────

    /// <summary>Adds a phone number to a contact.</summary>
    [HttpPost("api/contacts/{contactUid:guid}/phones")]
    [ProducesResponseType(typeof(PhoneNumberDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddPhone(Guid contactUid, [FromBody] ContactPhoneRequest request)
    {
        var contact = await _contacts.GetByUidAsync(contactUid);
        if (contact is null) return NotFound();

        var phone = await _contacts.AddPhoneNumberAsync(contactUid, request.Number, request.Type, request.IsPrimary, request.Tags);
        return StatusCode(StatusCodes.Status201Created, PhoneNumberDto.From(phone));
    }

    /// <summary>Updates an existing phone number.</summary>
    [HttpPut("api/contacts/phones/{uid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePhone(Guid uid, [FromBody] ContactPhoneRequest request)
    {
        var contacts = await _contacts.GetAllAsync();
        var phone = contacts.SelectMany(c => c.PhoneNumbers).FirstOrDefault(p => p.Uid == uid);
        if (phone is null) return NotFound();

        phone.Number    = request.Number;
        phone.Type      = request.Type;
        phone.IsPrimary = request.IsPrimary;
        phone.Tags      = request.Tags;

        await _contacts.UpdatePhoneNumberAsync(phone);
        return NoContent();
    }

    /// <summary>Deletes a phone number by UID.</summary>
    [HttpDelete("api/contacts/phones/{uid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeletePhone(Guid uid)
    {
        await _contacts.DeletePhoneNumberAsync(uid);
        return NoContent();
    }

    // ── Postal addresses ───────────────────────────────────────────────────

    /// <summary>Adds a postal address to a contact.</summary>
    [HttpPost("api/contacts/{contactUid:guid}/addresses")]
    [ProducesResponseType(typeof(ContactAddressDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddAddress(Guid contactUid, [FromBody] ContactAddressRequest request)
    {
        var contact = await _contacts.GetByUidAsync(contactUid);
        if (contact is null) return NotFound();

        var address = new ContactAddress
        {
            Uid        = Guid.CreateVersion7(),
            ContactUid = contactUid,
            Street     = request.Street,
            City       = request.City,
            State      = request.State,
            PostalCode = request.PostalCode,
            Country    = request.Country,
            Type       = request.Type,
            IsPrimary  = request.IsPrimary,
            Tags       = request.Tags,
            CreatedAt  = DateTime.UtcNow,
            UpdatedAt  = DateTime.UtcNow,
        };

        var saved = await _contacts.AddAddressAsync(contactUid, address);
        return StatusCode(StatusCodes.Status201Created, ContactAddressDto.From(saved));
    }

    /// <summary>Updates an existing postal address.</summary>
    [HttpPut("api/contacts/addresses/{uid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAddress(Guid uid, [FromBody] ContactAddressRequest request)
    {
        var contacts = await _contacts.GetAllAsync();
        var address = contacts.SelectMany(c => c.Addresses).FirstOrDefault(a => a.Uid == uid);
        if (address is null) return NotFound();

        address.Street     = request.Street;
        address.City       = request.City;
        address.State      = request.State;
        address.PostalCode = request.PostalCode;
        address.Country    = request.Country;
        address.Type       = request.Type;
        address.IsPrimary  = request.IsPrimary;
        address.Tags       = request.Tags;

        await _contacts.UpdateAddressAsync(address);
        return NoContent();
    }

    /// <summary>Deletes a postal address by UID.</summary>
    [HttpDelete("api/contacts/addresses/{uid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAddress(Guid uid)
    {
        await _contacts.DeleteAddressAsync(uid);
        return NoContent();
    }
}
