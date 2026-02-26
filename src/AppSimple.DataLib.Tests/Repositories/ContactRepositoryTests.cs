using AppSimple.Core.Enums;
using AppSimple.Core.Models;
using AppSimple.DataLib.Repositories;
using AppSimple.DataLib.Tests.Helpers;

namespace AppSimple.DataLib.Tests.Repositories;

/// <summary>
/// Integration tests for <see cref="ContactRepository"/> using an in-memory SQLite database.
/// Tests cover the contact root entity and all child collections
/// (email addresses, phone numbers, postal addresses).
/// </summary>
public sealed class ContactRepositoryTests : DatabaseTestBase
{
    private readonly ContactRepository _repo;
    private readonly UserRepository    _userRepo;

    private readonly Guid _ownerUid;

    public ContactRepositoryTests()
    {
        _repo     = new ContactRepository(ConnectionFactory);
        _userRepo = new UserRepository(ConnectionFactory);

        // Contacts have a FK to Users — seed an owner user up front
        var owner = UserFactory.Create();
        _ownerUid = owner.Uid;
        _userRepo.AddAsync(owner).GetAwaiter().GetResult();
    }

    // -------------------------------------------------------------------------
    // AddAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddAsync_InsertsContact_CanBeRetrievedByUid()
    {
        var contact = ContactFactory.Create(ownerUserUid: _ownerUid);
        await _repo.AddAsync(contact);

        var result = await _repo.GetByUidAsync(contact.Uid);

        Assert.NotNull(result);
        Assert.Equal(contact.Uid, result.Uid);
    }

    [Fact]
    public async Task AddAsync_PersistsAllFields()
    {
        var contact = ContactFactory.Create(
            ownerUserUid: _ownerUid,
            name: "Jane Doe",
            tags: ["friend", "work"]);
        await _repo.AddAsync(contact);

        var result = await _repo.GetByUidAsync(contact.Uid);

        Assert.NotNull(result);
        Assert.Equal(_ownerUid, result.OwnerUserUid);
        Assert.Equal("Jane Doe", result.Name);
        Assert.Equal(["friend", "work"], result.Tags);
    }

    [Fact]
    public async Task AddAsync_EmptyTags_RoundTripsCorrectly()
    {
        var contact = ContactFactory.Create(ownerUserUid: _ownerUid, tags: []);
        await _repo.AddAsync(contact);

        var result = await _repo.GetByUidAsync(contact.Uid);

        Assert.NotNull(result);
        Assert.Empty(result.Tags);
    }

    // -------------------------------------------------------------------------
    // GetByUidAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByUidAsync_ReturnsNull_WhenNotFound()
    {
        Assert.Null(await _repo.GetByUidAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetByUidAsync_PopulatesChildCollections()
    {
        var contact = ContactFactory.Create(ownerUserUid: _ownerUid);
        await _repo.AddAsync(contact);

        var email   = ContactFactory.CreateEmail(contact.Uid, "jane@example.com");
        var phone   = ContactFactory.CreatePhone(contact.Uid, "+1-555-0001");
        var address = ContactFactory.CreateAddress(contact.Uid, "1 Main St");
        await _repo.AddEmailAddressAsync(email);
        await _repo.AddPhoneNumberAsync(phone);
        await _repo.AddAddressAsync(address);

        var result = await _repo.GetByUidAsync(contact.Uid);

        Assert.NotNull(result);
        Assert.Single(result.EmailAddresses);
        Assert.Single(result.PhoneNumbers);
        Assert.Single(result.Addresses);
    }

    // -------------------------------------------------------------------------
    // GetAllAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNoContacts()
    {
        Assert.Empty(await _repo.GetAllAsync());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllInsertedContacts()
    {
        await _repo.AddAsync(ContactFactory.Create(ownerUserUid: _ownerUid));
        await _repo.AddAsync(ContactFactory.Create(ownerUserUid: _ownerUid));
        await _repo.AddAsync(ContactFactory.Create(ownerUserUid: _ownerUid));

        Assert.Equal(3, (await _repo.GetAllAsync()).Count());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsContactsOrderedByName()
    {
        await _repo.AddAsync(ContactFactory.Create(ownerUserUid: _ownerUid, name: "Zara"));
        await _repo.AddAsync(ContactFactory.Create(ownerUserUid: _ownerUid, name: "Alice"));
        await _repo.AddAsync(ContactFactory.Create(ownerUserUid: _ownerUid, name: "Mike"));

        var names = (await _repo.GetAllAsync()).Select(c => c.Name).ToList();

        Assert.Equal(["Alice", "Mike", "Zara"], names);
    }

    // -------------------------------------------------------------------------
    // GetByOwnerUidAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByOwnerUidAsync_ReturnsOnlyContactsForOwner()
    {
        var otherOwner = UserFactory.Create();
        await _userRepo.AddAsync(otherOwner);

        await _repo.AddAsync(ContactFactory.Create(ownerUserUid: _ownerUid));
        await _repo.AddAsync(ContactFactory.Create(ownerUserUid: _ownerUid));
        await _repo.AddAsync(ContactFactory.Create(ownerUserUid: otherOwner.Uid));

        var result = await _repo.GetByOwnerUidAsync(_ownerUid);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByOwnerUidAsync_ReturnsEmpty_WhenNoContactsForOwner()
    {
        Assert.Empty(await _repo.GetByOwnerUidAsync(Guid.NewGuid()));
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_PersistsChangedFields()
    {
        var contact = ContactFactory.Create(ownerUserUid: _ownerUid, name: "Old Name");
        await _repo.AddAsync(contact);

        contact.Name      = "New Name";
        contact.Tags      = ["updated"];
        contact.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(contact);

        var result = await _repo.GetByUidAsync(contact.Uid);
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        Assert.Equal(["updated"], result.Tags);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_RemovesContact()
    {
        var contact = ContactFactory.Create(ownerUserUid: _ownerUid);
        await _repo.AddAsync(contact);

        await _repo.DeleteAsync(contact.Uid);

        Assert.Null(await _repo.GetByUidAsync(contact.Uid));
    }

    [Fact]
    public async Task DeleteAsync_DoesNotThrow_WhenNotFound()
    {
        var ex = await Record.ExceptionAsync(() => _repo.DeleteAsync(Guid.NewGuid()));
        Assert.Null(ex);
    }

    // -------------------------------------------------------------------------
    // Email addresses
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddEmailAddressAsync_PersistsAllFields()
    {
        var contact = ContactFactory.Create(ownerUserUid: _ownerUid);
        await _repo.AddAsync(contact);

        var email = ContactFactory.CreateEmail(
            contact.Uid, "work@example.com", EmailType.Work, isPrimary: true, tags: ["billing"]);
        await _repo.AddEmailAddressAsync(email);

        var result = await _repo.GetByUidAsync(contact.Uid);
        var saved  = result!.EmailAddresses.Single();

        Assert.Equal("work@example.com", saved.Email);
        Assert.Equal(EmailType.Work, saved.Type);
        Assert.True(saved.IsPrimary);
        Assert.Equal(["billing"], saved.Tags);
    }

    [Fact]
    public async Task UpdateEmailAddressAsync_PersistsChangedFields()
    {
        var contact = ContactFactory.Create(ownerUserUid: _ownerUid);
        await _repo.AddAsync(contact);

        var email = ContactFactory.CreateEmail(contact.Uid, "old@example.com");
        await _repo.AddEmailAddressAsync(email);

        email.Email     = "new@example.com";
        email.IsPrimary = true;
        email.Type      = EmailType.Work;
        email.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateEmailAddressAsync(email);

        var result = await _repo.GetByUidAsync(contact.Uid);
        var saved  = result!.EmailAddresses.Single();

        Assert.Equal("new@example.com", saved.Email);
        Assert.True(saved.IsPrimary);
        Assert.Equal(EmailType.Work, saved.Type);
    }

    [Fact]
    public async Task DeleteEmailAddressAsync_RemovesEmail()
    {
        var contact = ContactFactory.Create(ownerUserUid: _ownerUid);
        await _repo.AddAsync(contact);

        var email = ContactFactory.CreateEmail(contact.Uid);
        await _repo.AddEmailAddressAsync(email);

        await _repo.DeleteEmailAddressAsync(email.Uid);

        var result = await _repo.GetByUidAsync(contact.Uid);
        Assert.Empty(result!.EmailAddresses);
    }

    // -------------------------------------------------------------------------
    // Phone numbers
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddPhoneNumberAsync_PersistsAllFields()
    {
        var contact = ContactFactory.Create(ownerUserUid: _ownerUid);
        await _repo.AddAsync(contact);

        var phone = ContactFactory.CreatePhone(
            contact.Uid, "+1-555-9999", PhoneType.Work, isPrimary: true, tags: ["main"]);
        await _repo.AddPhoneNumberAsync(phone);

        var result = await _repo.GetByUidAsync(contact.Uid);
        var saved  = result!.PhoneNumbers.Single();

        Assert.Equal("+1-555-9999", saved.Number);
        Assert.Equal(PhoneType.Work, saved.Type);
        Assert.True(saved.IsPrimary);
        Assert.Equal(["main"], saved.Tags);
    }

    [Fact]
    public async Task UpdatePhoneNumberAsync_PersistsChangedFields()
    {
        var contact = ContactFactory.Create(ownerUserUid: _ownerUid);
        await _repo.AddAsync(contact);

        var phone = ContactFactory.CreatePhone(contact.Uid, "+1-555-0001");
        await _repo.AddPhoneNumberAsync(phone);

        phone.Number    = "+1-555-9999";
        phone.IsPrimary = true;
        phone.Type      = PhoneType.Home;
        phone.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdatePhoneNumberAsync(phone);

        var result = await _repo.GetByUidAsync(contact.Uid);
        var saved  = result!.PhoneNumbers.Single();

        Assert.Equal("+1-555-9999", saved.Number);
        Assert.True(saved.IsPrimary);
        Assert.Equal(PhoneType.Home, saved.Type);
    }

    [Fact]
    public async Task DeletePhoneNumberAsync_RemovesPhone()
    {
        var contact = ContactFactory.Create(ownerUserUid: _ownerUid);
        await _repo.AddAsync(contact);

        var phone = ContactFactory.CreatePhone(contact.Uid);
        await _repo.AddPhoneNumberAsync(phone);

        await _repo.DeletePhoneNumberAsync(phone.Uid);

        var result = await _repo.GetByUidAsync(contact.Uid);
        Assert.Empty(result!.PhoneNumbers);
    }

    // -------------------------------------------------------------------------
    // Postal addresses
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddAddressAsync_PersistsAllFields()
    {
        var contact = ContactFactory.Create(ownerUserUid: _ownerUid);
        await _repo.AddAsync(contact);

        var address = ContactFactory.CreateAddress(
            contact.Uid, "123 Main St", AddressType.Work, isPrimary: true);
        await _repo.AddAddressAsync(address);

        var result = await _repo.GetByUidAsync(contact.Uid);
        var saved  = result!.Addresses.Single();

        Assert.Equal("123 Main St", saved.Street);
        Assert.Equal("TestCity", saved.City);
        Assert.Equal("US", saved.Country);
        Assert.Equal(AddressType.Work, saved.Type);
        Assert.True(saved.IsPrimary);
    }

    [Fact]
    public async Task UpdateAddressAsync_PersistsChangedFields()
    {
        var contact = ContactFactory.Create(ownerUserUid: _ownerUid);
        await _repo.AddAsync(contact);

        var address = ContactFactory.CreateAddress(contact.Uid, "1 Old St");
        await _repo.AddAddressAsync(address);

        address.Street    = "2 New St";
        address.City      = "NewCity";
        address.IsPrimary = true;
        address.Type      = AddressType.Work;
        address.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAddressAsync(address);

        var result = await _repo.GetByUidAsync(contact.Uid);
        var saved  = result!.Addresses.Single();

        Assert.Equal("2 New St", saved.Street);
        Assert.Equal("NewCity", saved.City);
        Assert.True(saved.IsPrimary);
        Assert.Equal(AddressType.Work, saved.Type);
    }

    [Fact]
    public async Task DeleteAddressAsync_RemovesAddress()
    {
        var contact = ContactFactory.Create(ownerUserUid: _ownerUid);
        await _repo.AddAsync(contact);

        var address = ContactFactory.CreateAddress(contact.Uid);
        await _repo.AddAddressAsync(address);

        await _repo.DeleteAddressAsync(address.Uid);

        var result = await _repo.GetByUidAsync(contact.Uid);
        Assert.Empty(result!.Addresses);
    }

    [Fact]
    public async Task Contact_CanHaveMultipleChildEntities()
    {
        var contact = ContactFactory.Create(ownerUserUid: _ownerUid);
        await _repo.AddAsync(contact);

        await _repo.AddEmailAddressAsync(ContactFactory.CreateEmail(contact.Uid, "a@b.com"));
        await _repo.AddEmailAddressAsync(ContactFactory.CreateEmail(contact.Uid, "b@c.com"));
        await _repo.AddPhoneNumberAsync(ContactFactory.CreatePhone(contact.Uid, "+1-1"));
        await _repo.AddPhoneNumberAsync(ContactFactory.CreatePhone(contact.Uid, "+1-2"));
        await _repo.AddAddressAsync(ContactFactory.CreateAddress(contact.Uid, "Street 1"));
        await _repo.AddAddressAsync(ContactFactory.CreateAddress(contact.Uid, "Street 2"));

        var result = await _repo.GetByUidAsync(contact.Uid);

        Assert.NotNull(result);
        Assert.Equal(2, result.EmailAddresses.Count);
        Assert.Equal(2, result.PhoneNumbers.Count);
        Assert.Equal(2, result.Addresses.Count);
    }
}
