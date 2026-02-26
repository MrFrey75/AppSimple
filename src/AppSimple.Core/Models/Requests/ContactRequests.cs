using AppSimple.Core.Enums;

namespace AppSimple.Core.Models.Requests;

/// <summary>Request payload for creating a new contact.</summary>
public sealed class CreateContactRequest
{
    /// <summary>Gets or sets the display name of the contact.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets optional string tags for the contact.</summary>
    public List<string> Tags { get; set; } = [];
}

/// <summary>Request payload for updating an existing contact's top-level fields.</summary>
public sealed class UpdateContactRequest
{
    /// <summary>Gets or sets the updated name. <c>null</c> means no change.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the updated tag list. <c>null</c> means no change.</summary>
    public List<string>? Tags { get; set; }
}

/// <summary>Request payload for adding or updating an email address on a contact.</summary>
public sealed class ContactEmailRequest
{
    /// <summary>Gets or sets the email address string.</summary>
    public required string Email { get; set; }

    /// <summary>Gets or sets the email type.</summary>
    public EmailType Type { get; set; } = EmailType.Personal;

    /// <summary>Gets or sets whether this is the primary email address.</summary>
    public bool IsPrimary { get; set; }

    /// <summary>Gets or sets optional string tags.</summary>
    public List<string> Tags { get; set; } = [];
}

/// <summary>Request payload for adding or updating a phone number on a contact.</summary>
public sealed class ContactPhoneRequest
{
    /// <summary>Gets or sets the phone number string.</summary>
    public required string Number { get; set; }

    /// <summary>Gets or sets the phone type.</summary>
    public PhoneType Type { get; set; } = PhoneType.Mobile;

    /// <summary>Gets or sets whether this is the primary phone number.</summary>
    public bool IsPrimary { get; set; }

    /// <summary>Gets or sets optional string tags.</summary>
    public List<string> Tags { get; set; } = [];
}

/// <summary>Request payload for adding or updating a postal address on a contact.</summary>
public sealed class ContactAddressRequest
{
    /// <summary>Gets or sets the street line.</summary>
    public required string Street { get; set; }

    /// <summary>Gets or sets the city.</summary>
    public required string City { get; set; }

    /// <summary>Gets or sets the state or province.</summary>
    public string State { get; set; } = string.Empty;

    /// <summary>Gets or sets the postal or ZIP code.</summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the country.</summary>
    public required string Country { get; set; }

    /// <summary>Gets or sets the address type.</summary>
    public AddressType Type { get; set; } = AddressType.Home;

    /// <summary>Gets or sets whether this is the primary address.</summary>
    public bool IsPrimary { get; set; }

    /// <summary>Gets or sets optional string tags.</summary>
    public List<string> Tags { get; set; } = [];
}
