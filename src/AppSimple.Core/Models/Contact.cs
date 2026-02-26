using AppSimple.Core.Enums;

namespace AppSimple.Core.Models;

/// <summary>Represents a contact owned by a user.</summary>
public class Contact : BaseEntity
{
    /// <summary>Gets or sets the UID of the user who owns this contact.</summary>
    public required Guid OwnerUserUid { get; set; }

    /// <summary>Gets or sets the display name of the contact.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets free-form string labels for this contact.</summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>Gets or sets the email addresses associated with this contact.</summary>
    public List<EmailAddress> EmailAddresses { get; set; } = [];

    /// <summary>Gets or sets the phone numbers associated with this contact.</summary>
    public List<PhoneNumber> PhoneNumbers { get; set; } = [];

    /// <summary>Gets or sets the postal addresses associated with this contact.</summary>
    public List<ContactAddress> Addresses { get; set; } = [];
}

/// <summary>Represents an email address belonging to a <see cref="Contact"/>.</summary>
public class EmailAddress : BaseEntity
{
    /// <summary>Gets or sets the UID of the parent contact.</summary>
    public required Guid ContactUid { get; set; }

    /// <summary>Gets or sets the email address string.</summary>
    public required string Email { get; set; }

    /// <summary>Gets or sets a value indicating whether this is the primary email address.</summary>
    public bool IsPrimary { get; set; }

    /// <summary>Gets or sets free-form string labels for this email address.</summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>Gets or sets the type of this email address.</summary>
    public EmailType Type { get; set; } = EmailType.Personal;
}

/// <summary>Represents a phone number belonging to a <see cref="Contact"/>.</summary>
public class PhoneNumber : BaseEntity
{
    /// <summary>Gets or sets the UID of the parent contact.</summary>
    public required Guid ContactUid { get; set; }

    /// <summary>Gets or sets the phone number string.</summary>
    public required string Number { get; set; }

    /// <summary>Gets or sets a value indicating whether this is the primary phone number.</summary>
    public bool IsPrimary { get; set; }

    /// <summary>Gets or sets free-form string labels for this phone number.</summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>Gets or sets the type of this phone number.</summary>
    public PhoneType Type { get; set; } = PhoneType.Mobile;
}

/// <summary>Represents a postal address belonging to a <see cref="Contact"/>.</summary>
public class ContactAddress : BaseEntity
{
    /// <summary>Gets or sets the UID of the parent contact.</summary>
    public required Guid ContactUid { get; set; }

    /// <summary>Gets or sets the street line of the address.</summary>
    public required string Street { get; set; }

    /// <summary>Gets or sets the city.</summary>
    public required string City { get; set; }

    /// <summary>Gets or sets the state or province.</summary>
    public string State { get; set; } = string.Empty;

    /// <summary>Gets or sets the postal or ZIP code.</summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the country.</summary>
    public required string Country { get; set; }

    /// <summary>Gets or sets a value indicating whether this is the primary address.</summary>
    public bool IsPrimary { get; set; }

    /// <summary>Gets or sets free-form string labels for this address.</summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>Gets or sets the type of this address.</summary>
    public AddressType Type { get; set; } = AddressType.Home;
}
