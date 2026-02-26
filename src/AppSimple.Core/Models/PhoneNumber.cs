using AppSimple.Core.Enums;

namespace AppSimple.Core.Models;

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
