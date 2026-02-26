using AppSimple.Core.Enums;

namespace AppSimple.Core.Models;

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
