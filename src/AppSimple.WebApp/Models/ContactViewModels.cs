using AppSimple.Core.Enums;
using AppSimple.Core.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace AppSimple.WebApp.Models;

/// <summary>ViewModel for the contacts list page.</summary>
public sealed class ContactListViewModel
{
    /// <summary>Gets or sets the list of contacts.</summary>
    public IReadOnlyList<ContactDto> Contacts { get; set; } = [];

    /// <summary>Gets or sets an error message, if any.</summary>
    public string? Error { get; set; }
}

/// <summary>ViewModel for the contact detail page.</summary>
public sealed class ContactDetailViewModel
{
    /// <summary>Gets or sets the contact being viewed.</summary>
    public required ContactDto Contact { get; set; }
}

/// <summary>ViewModel for creating a new contact.</summary>
public sealed class CreateContactViewModel
{
    /// <summary>Gets or sets the contact name.</summary>
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}

/// <summary>ViewModel for editing a contact's name.</summary>
public sealed class EditContactViewModel
{
    /// <summary>Gets or sets the contact UID.</summary>
    public Guid Uid { get; set; }

    /// <summary>Gets or sets the contact name.</summary>
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}

/// <summary>ViewModel for adding or editing an email address on a contact.</summary>
public sealed class ContactEmailViewModel
{
    /// <summary>Gets or sets the contact UID.</summary>
    public Guid ContactUid { get; set; }

    /// <summary>Gets or sets the email UID (empty when adding).</summary>
    public Guid EmailUid { get; set; }

    /// <summary>Gets or sets the email address.</summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the email type.</summary>
    public EmailType Type { get; set; } = EmailType.Personal;

    /// <summary>Gets or sets whether this is the primary email.</summary>
    public bool IsPrimary { get; set; }
}

/// <summary>ViewModel for adding or editing a phone number on a contact.</summary>
public sealed class ContactPhoneViewModel
{
    /// <summary>Gets or sets the contact UID.</summary>
    public Guid ContactUid { get; set; }

    /// <summary>Gets or sets the phone UID (empty when adding).</summary>
    public Guid PhoneUid { get; set; }

    /// <summary>Gets or sets the phone number.</summary>
    [Required(ErrorMessage = "Number is required.")]
    public string Number { get; set; } = string.Empty;

    /// <summary>Gets or sets the phone type.</summary>
    public PhoneType Type { get; set; } = PhoneType.Mobile;

    /// <summary>Gets or sets whether this is the primary phone.</summary>
    public bool IsPrimary { get; set; }
}

/// <summary>ViewModel for adding or editing a postal address on a contact.</summary>
public sealed class ContactAddressViewModel
{
    /// <summary>Gets or sets the contact UID.</summary>
    public Guid ContactUid { get; set; }

    /// <summary>Gets or sets the address UID (empty when adding).</summary>
    public Guid AddressUid { get; set; }

    /// <summary>Gets or sets the street line.</summary>
    [Required(ErrorMessage = "Street is required.")]
    public string Street { get; set; } = string.Empty;

    /// <summary>Gets or sets the city.</summary>
    [Required(ErrorMessage = "City is required.")]
    public string City { get; set; } = string.Empty;

    /// <summary>Gets or sets the state or province.</summary>
    public string State { get; set; } = string.Empty;

    /// <summary>Gets or sets the postal or ZIP code.</summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the country.</summary>
    [Required(ErrorMessage = "Country is required.")]
    public string Country { get; set; } = string.Empty;

    /// <summary>Gets or sets the address type.</summary>
    public AddressType Type { get; set; } = AddressType.Home;

    /// <summary>Gets or sets whether this is the primary address.</summary>
    public bool IsPrimary { get; set; }
}
