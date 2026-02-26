using AppSimple.Core.Enums;
using AppSimple.Core.Interfaces;
using AppSimple.Core.Models;
using AppSimple.DataLib.Db;
using Dapper;
using Serilog;

namespace AppSimple.DataLib.Repositories;

/// <summary>
/// SQLite/Dapper implementation of <see cref="IContactRepository"/>.
/// Child collections (emails, phones, addresses) are loaded via separate queries.
/// <see cref="List{string}"/> Tags columns are serialised as JSON TEXT.
/// </summary>
public sealed class ContactRepository : IContactRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger _logger = Log.ForContext<ContactRepository>();

    /// <summary>Initializes a new instance of <see cref="ContactRepository"/>.</summary>
    public ContactRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // ── IRepository<Contact> ─────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<Contact?> GetByUidAsync(Guid uid)
    {
        using var connection = _connectionFactory.CreateConnection();
        var contact = await connection.QuerySingleOrDefaultAsync<Contact>(
            "SELECT * FROM Contacts WHERE Uid = @Uid",
            new { Uid = uid.ToString() });

        if (contact is not null)
            await PopulateChildrenAsync(connection, contact);

        return contact;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Contact>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var contacts = (await connection.QueryAsync<Contact>(
            "SELECT * FROM Contacts ORDER BY Name")).ToList();

        foreach (var c in contacts)
            await PopulateChildrenAsync(connection, c);

        return contacts;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Contact>> GetByOwnerUidAsync(Guid ownerUserUid)
    {
        using var connection = _connectionFactory.CreateConnection();
        var contacts = (await connection.QueryAsync<Contact>(
            "SELECT * FROM Contacts WHERE OwnerUserUid = @OwnerUserUid ORDER BY Name",
            new { OwnerUserUid = ownerUserUid.ToString() })).ToList();

        foreach (var c in contacts)
            await PopulateChildrenAsync(connection, c);

        return contacts;
    }

    /// <inheritdoc />
    public async Task AddAsync(Contact entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO Contacts (Uid, OwnerUserUid, Name, Tags, IsSystem, CreatedAt, UpdatedAt)
            VALUES (@Uid, @OwnerUserUid, @Name, @Tags, @IsSystem, @CreatedAt, @UpdatedAt)
            """, MapContact(entity));
        _logger.Information("Contact '{Name}' ({Uid}) created.", entity.Name, entity.Uid);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Contact entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("""
            UPDATE Contacts SET Name = @Name, Tags = @Tags, UpdatedAt = @UpdatedAt
            WHERE Uid = @Uid
            """, MapContact(entity));
        _logger.Information("Contact {Uid} updated.", entity.Uid);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid uid)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "DELETE FROM Contacts WHERE Uid = @Uid",
            new { Uid = uid.ToString() });
        _logger.Information("Contact {Uid} deleted.", uid);
    }

    // ── Email addresses ───────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task AddEmailAddressAsync(EmailAddress email)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO ContactEmailAddresses (Uid, ContactUid, Email, IsPrimary, Tags, Type, IsSystem, CreatedAt, UpdatedAt)
            VALUES (@Uid, @ContactUid, @Email, @IsPrimary, @Tags, @Type, @IsSystem, @CreatedAt, @UpdatedAt)
            """, MapEmail(email));
    }

    /// <inheritdoc />
    public async Task UpdateEmailAddressAsync(EmailAddress email)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("""
            UPDATE ContactEmailAddresses SET Email = @Email, IsPrimary = @IsPrimary, Tags = @Tags, Type = @Type, UpdatedAt = @UpdatedAt
            WHERE Uid = @Uid
            """, MapEmail(email));
    }

    /// <inheritdoc />
    public async Task DeleteEmailAddressAsync(Guid uid)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "DELETE FROM ContactEmailAddresses WHERE Uid = @Uid",
            new { Uid = uid.ToString() });
    }

    // ── Phone numbers ─────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task AddPhoneNumberAsync(PhoneNumber phone)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO ContactPhoneNumbers (Uid, ContactUid, Number, IsPrimary, Tags, Type, IsSystem, CreatedAt, UpdatedAt)
            VALUES (@Uid, @ContactUid, @Number, @IsPrimary, @Tags, @Type, @IsSystem, @CreatedAt, @UpdatedAt)
            """, MapPhone(phone));
    }

    /// <inheritdoc />
    public async Task UpdatePhoneNumberAsync(PhoneNumber phone)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("""
            UPDATE ContactPhoneNumbers SET Number = @Number, IsPrimary = @IsPrimary, Tags = @Tags, Type = @Type, UpdatedAt = @UpdatedAt
            WHERE Uid = @Uid
            """, MapPhone(phone));
    }

    /// <inheritdoc />
    public async Task DeletePhoneNumberAsync(Guid uid)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "DELETE FROM ContactPhoneNumbers WHERE Uid = @Uid",
            new { Uid = uid.ToString() });
    }

    // ── Postal addresses ──────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task AddAddressAsync(ContactAddress address)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO ContactAddresses (Uid, ContactUid, Street, City, State, PostalCode, Country, IsPrimary, Tags, Type, IsSystem, CreatedAt, UpdatedAt)
            VALUES (@Uid, @ContactUid, @Street, @City, @State, @PostalCode, @Country, @IsPrimary, @Tags, @Type, @IsSystem, @CreatedAt, @UpdatedAt)
            """, MapAddress(address));
    }

    /// <inheritdoc />
    public async Task UpdateAddressAsync(ContactAddress address)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("""
            UPDATE ContactAddresses SET Street = @Street, City = @City, State = @State, PostalCode = @PostalCode, Country = @Country,
                IsPrimary = @IsPrimary, Tags = @Tags, Type = @Type, UpdatedAt = @UpdatedAt
            WHERE Uid = @Uid
            """, MapAddress(address));
    }

    /// <inheritdoc />
    public async Task DeleteAddressAsync(Guid uid)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "DELETE FROM ContactAddresses WHERE Uid = @Uid",
            new { Uid = uid.ToString() });
    }

    // ── helpers ───────────────────────────────────────────────────────────

    private static async Task PopulateChildrenAsync(System.Data.IDbConnection connection, Contact contact)
    {
        var uid = contact.Uid.ToString();
        contact.EmailAddresses = (await connection.QueryAsync<EmailAddress>(
            "SELECT * FROM ContactEmailAddresses WHERE ContactUid = @Uid", new { Uid = uid })).ToList();
        contact.PhoneNumbers   = (await connection.QueryAsync<PhoneNumber>(
            "SELECT * FROM ContactPhoneNumbers WHERE ContactUid = @Uid", new { Uid = uid })).ToList();
        contact.Addresses      = (await connection.QueryAsync<ContactAddress>(
            "SELECT * FROM ContactAddresses WHERE ContactUid = @Uid", new { Uid = uid })).ToList();
    }

    private static object MapContact(Contact c) => new
    {
        Uid          = c.Uid.ToString(),
        OwnerUserUid = c.OwnerUserUid.ToString(),
        c.Name,
        Tags         = c.Tags,
        IsSystem     = c.IsSystem ? 1 : 0,
        CreatedAt    = c.CreatedAt.ToString("O"),
        UpdatedAt    = c.UpdatedAt.ToString("O"),
    };

    private static object MapEmail(EmailAddress e) => new
    {
        Uid        = e.Uid.ToString(),
        ContactUid = e.ContactUid.ToString(),
        e.Email,
        IsPrimary  = e.IsPrimary ? 1 : 0,
        Tags       = e.Tags,
        Type       = (int)e.Type,
        IsSystem   = e.IsSystem ? 1 : 0,
        CreatedAt  = e.CreatedAt.ToString("O"),
        UpdatedAt  = e.UpdatedAt.ToString("O"),
    };

    private static object MapPhone(PhoneNumber p) => new
    {
        Uid        = p.Uid.ToString(),
        ContactUid = p.ContactUid.ToString(),
        p.Number,
        IsPrimary  = p.IsPrimary ? 1 : 0,
        Tags       = p.Tags,
        Type       = (int)p.Type,
        IsSystem   = p.IsSystem ? 1 : 0,
        CreatedAt  = p.CreatedAt.ToString("O"),
        UpdatedAt  = p.UpdatedAt.ToString("O"),
    };

    private static object MapAddress(ContactAddress a) => new
    {
        Uid        = a.Uid.ToString(),
        ContactUid = a.ContactUid.ToString(),
        a.Street,
        a.City,
        a.State,
        a.PostalCode,
        a.Country,
        IsPrimary  = a.IsPrimary ? 1 : 0,
        Tags       = a.Tags,
        Type       = (int)a.Type,
        IsSystem   = a.IsSystem ? 1 : 0,
        CreatedAt  = a.CreatedAt.ToString("O"),
        UpdatedAt  = a.UpdatedAt.ToString("O"),
    };
}
