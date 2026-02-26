using AppSimple.Core.Enums;

namespace AppSimple.Core.Models;

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
