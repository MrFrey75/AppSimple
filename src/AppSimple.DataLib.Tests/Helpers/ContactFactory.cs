using AppSimple.Core.Enums;
using AppSimple.Core.Models;

namespace AppSimple.DataLib.Tests.Helpers;

/// <summary>
/// Provides factory methods for building <see cref="Contact"/> and child entity test instances.
/// </summary>
internal static class ContactFactory
{
    private static int _counter;

    /// <summary>Creates a new <see cref="Contact"/> with unique UID and optional overrides.</summary>
    public static Contact Create(
        Guid? ownerUserUid = null,
        string? name = null,
        List<string>? tags = null)
    {
        var n   = ++_counter;
        var now = DateTime.UtcNow;
        return new Contact
        {
            Uid          = Guid.CreateVersion7(),
            OwnerUserUid = ownerUserUid ?? Guid.CreateVersion7(),
            Name         = name         ?? $"Contact {n}",
            Tags         = tags         ?? [],
            CreatedAt    = now,
            UpdatedAt    = now,
        };
    }

    /// <summary>Creates a new <see cref="EmailAddress"/> for the given contact.</summary>
    public static EmailAddress CreateEmail(
        Guid contactUid,
        string? email = null,
        EmailType type = EmailType.Personal,
        bool isPrimary = false,
        List<string>? tags = null)
    {
        var n   = ++_counter;
        var now = DateTime.UtcNow;
        return new EmailAddress
        {
            Uid        = Guid.CreateVersion7(),
            ContactUid = contactUid,
            Email      = email     ?? $"test{n}@example.com",
            Type       = type,
            IsPrimary  = isPrimary,
            Tags       = tags      ?? [],
            CreatedAt  = now,
            UpdatedAt  = now,
        };
    }

    /// <summary>Creates a new <see cref="PhoneNumber"/> for the given contact.</summary>
    public static PhoneNumber CreatePhone(
        Guid contactUid,
        string? number = null,
        PhoneType type = PhoneType.Mobile,
        bool isPrimary = false,
        List<string>? tags = null)
    {
        var n   = ++_counter;
        var now = DateTime.UtcNow;
        return new PhoneNumber
        {
            Uid        = Guid.CreateVersion7(),
            ContactUid = contactUid,
            Number     = number    ?? $"+1-555-{n:D4}",
            Type       = type,
            IsPrimary  = isPrimary,
            Tags       = tags      ?? [],
            CreatedAt  = now,
            UpdatedAt  = now,
        };
    }

    /// <summary>Creates a new <see cref="ContactAddress"/> for the given contact.</summary>
    public static ContactAddress CreateAddress(
        Guid contactUid,
        string? street = null,
        AddressType type = AddressType.Home,
        bool isPrimary = false)
    {
        var n   = ++_counter;
        var now = DateTime.UtcNow;
        return new ContactAddress
        {
            Uid        = Guid.CreateVersion7(),
            ContactUid = contactUid,
            Street     = street     ?? $"{n} Test Street",
            City       = "TestCity",
            State      = "TS",
            PostalCode = "00000",
            Country    = "US",
            Type       = type,
            IsPrimary  = isPrimary,
            Tags       = [],
            CreatedAt  = now,
            UpdatedAt  = now,
        };
    }
}
