using AppSimple.Core.Models.Requests;
using AppSimple.WebApp.Models;
using AppSimple.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppSimple.WebApp.Controllers;

/// <summary>Controller for contacts pages.</summary>
[Authorize]
[Route("contacts")]
public sealed class ContactsController : Controller
{
    private readonly IApiClient _api;
    private readonly ILogger<ContactsController> _logger;

    /// <summary>Initializes a new instance of <see cref="ContactsController"/>.</summary>
    public ContactsController(IApiClient api, ILogger<ContactsController> logger)
    {
        _api    = api;
        _logger = logger;
    }

    private string? GetToken() => User.FindFirstValue("jwt_token");

    // ── Contacts ──────────────────────────────────────────────────────────

    /// <summary>Lists all contacts for the current user.</summary>
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var contacts = await _api.GetMyContactsAsync(token);
        return View(new ContactListViewModel { Contacts = contacts });
    }

    /// <summary>Displays a single contact with all child collections.</summary>
    [HttpGet("{uid:guid}")]
    public async Task<IActionResult> Detail(Guid uid)
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var contact = await _api.GetContactAsync(token, uid);
        if (contact is null)
        {
            TempData["Error"] = "Contact not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(new ContactDetailViewModel { Contact = contact });
    }

    /// <summary>Displays the create contact form.</summary>
    [HttpGet("create")]
    public IActionResult Create() => View(new CreateContactViewModel());

    /// <summary>Processes the create contact form.</summary>
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateContactViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var result = await _api.CreateContactAsync(token, new CreateContactRequest { Name = model.Name });
        if (result is null)
        {
            TempData["Error"] = "Failed to create contact.";
            return View(model);
        }

        _logger.LogInformation("User '{User}' created contact {Uid}", User.Identity?.Name, result.Uid);
        TempData["Success"] = $"Contact \"{result.Name}\" created.";
        return RedirectToAction(nameof(Detail), new { uid = result.Uid });
    }

    /// <summary>Displays the edit contact name form.</summary>
    [HttpGet("{uid:guid}/edit")]
    public async Task<IActionResult> Edit(Guid uid)
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var contact = await _api.GetContactAsync(token, uid);
        if (contact is null) return RedirectToAction(nameof(Index));

        return View(new EditContactViewModel { Uid = uid, Name = contact.Name });
    }

    /// <summary>Processes the edit contact name form.</summary>
    [HttpPost("{uid:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid uid, EditContactViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        await _api.UpdateContactAsync(token, uid, new UpdateContactRequest { Name = model.Name });
        TempData["Success"] = "Contact updated.";
        return RedirectToAction(nameof(Detail), new { uid });
    }

    /// <summary>Deletes a contact.</summary>
    [HttpPost("{uid:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid uid)
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        await _api.DeleteContactAsync(token, uid);
        TempData["Success"] = "Contact deleted.";
        return RedirectToAction(nameof(Index));
    }

    // ── Email addresses ───────────────────────────────────────────────────

    /// <summary>Displays the add email form.</summary>
    [HttpGet("{contactUid:guid}/emails/add")]
    public IActionResult AddEmail(Guid contactUid) =>
        View(new ContactEmailViewModel { ContactUid = contactUid });

    /// <summary>Processes the add email form.</summary>
    [HttpPost("{contactUid:guid}/emails/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEmail(Guid contactUid, ContactEmailViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        await _api.AddEmailAsync(token, contactUid, new ContactEmailRequest
        {
            Email     = model.Email,
            Type      = model.Type,
            IsPrimary = model.IsPrimary,
        });

        TempData["Success"] = "Email address added.";
        return RedirectToAction(nameof(Detail), new { uid = contactUid });
    }

    /// <summary>Displays the edit email form.</summary>
    [HttpGet("{contactUid:guid}/emails/{emailUid:guid}/edit")]
    public async Task<IActionResult> EditEmail(Guid contactUid, Guid emailUid)
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var contact = await _api.GetContactAsync(token, contactUid);
        var email = contact?.EmailAddresses.FirstOrDefault(e => e.Uid == emailUid);
        if (email is null) return RedirectToAction(nameof(Detail), new { uid = contactUid });

        return View(new ContactEmailViewModel
        {
            ContactUid = contactUid,
            EmailUid   = emailUid,
            Email      = email.Email,
            Type       = email.Type,
            IsPrimary  = email.IsPrimary,
        });
    }

    /// <summary>Processes the edit email form.</summary>
    [HttpPost("{contactUid:guid}/emails/{emailUid:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEmail(Guid contactUid, Guid emailUid, ContactEmailViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        await _api.UpdateEmailAsync(token, emailUid, new ContactEmailRequest
        {
            Email     = model.Email,
            Type      = model.Type,
            IsPrimary = model.IsPrimary,
        });

        TempData["Success"] = "Email address updated.";
        return RedirectToAction(nameof(Detail), new { uid = contactUid });
    }

    /// <summary>Deletes an email address.</summary>
    [HttpPost("{contactUid:guid}/emails/{emailUid:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEmail(Guid contactUid, Guid emailUid)
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        await _api.DeleteEmailAsync(token, emailUid);
        TempData["Success"] = "Email address deleted.";
        return RedirectToAction(nameof(Detail), new { uid = contactUid });
    }

    // ── Phone numbers ──────────────────────────────────────────────────────

    /// <summary>Displays the add phone form.</summary>
    [HttpGet("{contactUid:guid}/phones/add")]
    public IActionResult AddPhone(Guid contactUid) =>
        View(new ContactPhoneViewModel { ContactUid = contactUid });

    /// <summary>Processes the add phone form.</summary>
    [HttpPost("{contactUid:guid}/phones/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPhone(Guid contactUid, ContactPhoneViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        await _api.AddPhoneAsync(token, contactUid, new ContactPhoneRequest
        {
            Number    = model.Number,
            Type      = model.Type,
            IsPrimary = model.IsPrimary,
        });

        TempData["Success"] = "Phone number added.";
        return RedirectToAction(nameof(Detail), new { uid = contactUid });
    }

    /// <summary>Displays the edit phone form.</summary>
    [HttpGet("{contactUid:guid}/phones/{phoneUid:guid}/edit")]
    public async Task<IActionResult> EditPhone(Guid contactUid, Guid phoneUid)
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var contact = await _api.GetContactAsync(token, contactUid);
        var phone = contact?.PhoneNumbers.FirstOrDefault(p => p.Uid == phoneUid);
        if (phone is null) return RedirectToAction(nameof(Detail), new { uid = contactUid });

        return View(new ContactPhoneViewModel
        {
            ContactUid = contactUid,
            PhoneUid   = phoneUid,
            Number     = phone.Number,
            Type       = phone.Type,
            IsPrimary  = phone.IsPrimary,
        });
    }

    /// <summary>Processes the edit phone form.</summary>
    [HttpPost("{contactUid:guid}/phones/{phoneUid:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPhone(Guid contactUid, Guid phoneUid, ContactPhoneViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        await _api.UpdatePhoneAsync(token, phoneUid, new ContactPhoneRequest
        {
            Number    = model.Number,
            Type      = model.Type,
            IsPrimary = model.IsPrimary,
        });

        TempData["Success"] = "Phone number updated.";
        return RedirectToAction(nameof(Detail), new { uid = contactUid });
    }

    /// <summary>Deletes a phone number.</summary>
    [HttpPost("{contactUid:guid}/phones/{phoneUid:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhone(Guid contactUid, Guid phoneUid)
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        await _api.DeletePhoneAsync(token, phoneUid);
        TempData["Success"] = "Phone number deleted.";
        return RedirectToAction(nameof(Detail), new { uid = contactUid });
    }

    // ── Postal addresses ───────────────────────────────────────────────────

    /// <summary>Displays the add address form.</summary>
    [HttpGet("{contactUid:guid}/addresses/add")]
    public IActionResult AddAddress(Guid contactUid) =>
        View(new ContactAddressViewModel { ContactUid = contactUid });

    /// <summary>Processes the add address form.</summary>
    [HttpPost("{contactUid:guid}/addresses/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddress(Guid contactUid, ContactAddressViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        await _api.AddAddressAsync(token, contactUid, new ContactAddressRequest
        {
            Street     = model.Street,
            City       = model.City,
            State      = model.State,
            PostalCode = model.PostalCode,
            Country    = model.Country,
            Type       = model.Type,
            IsPrimary  = model.IsPrimary,
        });

        TempData["Success"] = "Address added.";
        return RedirectToAction(nameof(Detail), new { uid = contactUid });
    }

    /// <summary>Displays the edit address form.</summary>
    [HttpGet("{contactUid:guid}/addresses/{addressUid:guid}/edit")]
    public async Task<IActionResult> EditAddress(Guid contactUid, Guid addressUid)
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        var contact = await _api.GetContactAsync(token, contactUid);
        var address = contact?.Addresses.FirstOrDefault(a => a.Uid == addressUid);
        if (address is null) return RedirectToAction(nameof(Detail), new { uid = contactUid });

        return View(new ContactAddressViewModel
        {
            ContactUid = contactUid,
            AddressUid = addressUid,
            Street     = address.Street,
            City       = address.City,
            State      = address.State,
            PostalCode = address.PostalCode,
            Country    = address.Country,
            Type       = address.Type,
            IsPrimary  = address.IsPrimary,
        });
    }

    /// <summary>Processes the edit address form.</summary>
    [HttpPost("{contactUid:guid}/addresses/{addressUid:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAddress(Guid contactUid, Guid addressUid, ContactAddressViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        await _api.UpdateAddressAsync(token, addressUid, new ContactAddressRequest
        {
            Street     = model.Street,
            City       = model.City,
            State      = model.State,
            PostalCode = model.PostalCode,
            Country    = model.Country,
            Type       = model.Type,
            IsPrimary  = model.IsPrimary,
        });

        TempData["Success"] = "Address updated.";
        return RedirectToAction(nameof(Detail), new { uid = contactUid });
    }

    /// <summary>Deletes a postal address.</summary>
    [HttpPost("{contactUid:guid}/addresses/{addressUid:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress(Guid contactUid, Guid addressUid)
    {
        var token = GetToken();
        if (token is null) return RedirectToAction("Login", "Auth");

        await _api.DeleteAddressAsync(token, addressUid);
        TempData["Success"] = "Address deleted.";
        return RedirectToAction(nameof(Detail), new { uid = contactUid });
    }
}
