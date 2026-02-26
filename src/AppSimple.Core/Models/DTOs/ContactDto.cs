using AppSimple.Core.Enums;
using AppSimple.Core.Models;

namespace AppSimple.Core.Models.DTOs;

/// <summary>Read-only projection of a <see cref="Contact"/>.</summary>
public sealed class ContactDto
{
    /// <summary>Gets the contact's unique identifier.</summary>
    public Guid Uid { get; init; }

    /// <summary>Gets the UID of the owning user.</summary>
    public Guid OwnerUserUid { get; init; }

    /// <summary>Gets the contact's display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the string tags on the contact.</summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>Gets the email addresses.</summary>
    public IReadOnlyList<EmailAddressDto> EmailAddresses { get; init; } = [];

    /// <summary>Gets the phone numbers.</summary>
    public IReadOnlyList<PhoneNumberDto> PhoneNumbers { get; init; } = [];

    /// <summary>Gets the postal addresses.</summary>
    public IReadOnlyList<ContactAddressDto> Addresses { get; init; } = [];

    /// <summary>Gets the UTC timestamp when the contact was created.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Gets the UTC timestamp when the contact was last updated.</summary>
    public DateTime UpdatedAt { get; init; }

    /// <summary>Creates a <see cref="ContactDto"/> from a <see cref="Contact"/> entity.</summary>
    public static ContactDto From(Contact c) => new()
    {
        Uid            = c.Uid,
        OwnerUserUid   = c.OwnerUserUid,
        Name           = c.Name,
        Tags           = c.Tags,
        EmailAddresses = c.EmailAddresses.Select(EmailAddressDto.From).ToList(),
        PhoneNumbers   = c.PhoneNumbers.Select(PhoneNumberDto.From).ToList(),
        Addresses      = c.Addresses.Select(ContactAddressDto.From).ToList(),
        CreatedAt      = c.CreatedAt,
        UpdatedAt      = c.UpdatedAt,
    };
}

/// <summary>Read-only projection of an <see cref="EmailAddress"/>.</summary>
public sealed class EmailAddressDto
{
    /// <summary>Gets the unique identifier.</summary>
    public Guid Uid { get; init; }

    /// <summary>Gets the email address string.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Gets whether this is the primary email.</summary>
    public bool IsPrimary { get; init; }

    /// <summary>Gets the string tags.</summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>Gets the email type.</summary>
    public EmailType Type { get; init; }

    /// <summary>Creates an <see cref="EmailAddressDto"/> from an entity.</summary>
    public static EmailAddressDto From(EmailAddress e) => new()
    {
        Uid       = e.Uid,
        Email     = e.Email,
        IsPrimary = e.IsPrimary,
        Tags      = e.Tags,
        Type      = e.Type,
    };
}

/// <summary>Read-only projection of a <see cref="PhoneNumber"/>.</summary>
public sealed class PhoneNumberDto
{
    /// <summary>Gets the unique identifier.</summary>
    public Guid Uid { get; init; }

    /// <summary>Gets the phone number string.</summary>
    public string Number { get; init; } = string.Empty;

    /// <summary>Gets whether this is the primary phone number.</summary>
    public bool IsPrimary { get; init; }

    /// <summary>Gets the string tags.</summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>Gets the phone type.</summary>
    public PhoneType Type { get; init; }

    /// <summary>Creates a <see cref="PhoneNumberDto"/> from an entity.</summary>
    public static PhoneNumberDto From(PhoneNumber p) => new()
    {
        Uid       = p.Uid,
        Number    = p.Number,
        IsPrimary = p.IsPrimary,
        Tags      = p.Tags,
        Type      = p.Type,
    };
}

/// <summary>Read-only projection of a <see cref="ContactAddress"/>.</summary>
public sealed class ContactAddressDto
{
    /// <summary>Gets the unique identifier.</summary>
    public Guid Uid { get; init; }

    /// <summary>Gets the street line.</summary>
    public string Street { get; init; } = string.Empty;

    /// <summary>Gets the city.</summary>
    public string City { get; init; } = string.Empty;

    /// <summary>Gets the state or province.</summary>
    public string State { get; init; } = string.Empty;

    /// <summary>Gets the postal or ZIP code.</summary>
    public string PostalCode { get; init; } = string.Empty;

    /// <summary>Gets the country.</summary>
    public string Country { get; init; } = string.Empty;

    /// <summary>Gets whether this is the primary address.</summary>
    public bool IsPrimary { get; init; }

    /// <summary>Gets the string tags.</summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>Gets the address type.</summary>
    public AddressType Type { get; init; }

    /// <summary>Creates a <see cref="ContactAddressDto"/> from an entity.</summary>
    public static ContactAddressDto From(ContactAddress a) => new()
    {
        Uid        = a.Uid,
        Street     = a.Street,
        City       = a.City,
        State      = a.State,
        PostalCode = a.PostalCode,
        Country    = a.Country,
        IsPrimary  = a.IsPrimary,
        Tags       = a.Tags,
        Type       = a.Type,
    };
}
