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
