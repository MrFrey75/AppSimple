using AppSimple.Core.Enums;
using AppSimple.Core.Interfaces;
using AppSimple.Core.Logging;
using AppSimple.Core.Models;
using AppSimple.Core.Services.Impl;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace AppSimple.Core.Tests.Services;

/// <summary>Unit tests for <see cref="ContactService"/>.</summary>
public sealed class ContactServiceTests
{
    private readonly IContactRepository _repo = Substitute.For<IContactRepository>();
    private readonly IAppLogger<ContactService> _log = Substitute.For<IAppLogger<ContactService>>();
    private readonly ContactService _svc;

    public ContactServiceTests()
    {
        _svc = new ContactService(_repo, _log);
    }

    private static readonly Guid _ownerUid = Guid.CreateVersion7();

    private static Contact MakeContact(Guid? ownerUid = null) => new()
    {
        Uid          = Guid.CreateVersion7(),
        OwnerUserUid = ownerUid ?? _ownerUid,
        Name         = "Jane Doe",
        CreatedAt    = DateTime.UtcNow,
        UpdatedAt    = DateTime.UtcNow,
    };

    // -------------------------------------------------------------------------
    // GetByUidAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByUidAsync_ReturnsContact_WhenFound()
    {
        var contact = MakeContact();
        _repo.GetByUidAsync(contact.Uid).Returns(contact);

        var result = await _svc.GetByUidAsync(contact.Uid);
        Assert.Same(contact, result);
    }

    [Fact]
    public async Task GetByUidAsync_ReturnsNull_WhenNotFound()
    {
        _repo.GetByUidAsync(Arg.Any<Guid>()).ReturnsNull();
        Assert.Null(await _svc.GetByUidAsync(Guid.NewGuid()));
    }

    // -------------------------------------------------------------------------
    // GetAllAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_DelegatesToRepository()
    {
        var contacts = new[] { MakeContact(), MakeContact() };
        _repo.GetAllAsync().Returns(contacts);

        var result = await _svc.GetAllAsync();
        Assert.Equal(2, result.Count());
    }

    // -------------------------------------------------------------------------
    // GetByOwnerUidAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByOwnerUidAsync_ReturnsContactsForOwner()
    {
        var contacts = new[] { MakeContact(_ownerUid), MakeContact(_ownerUid) };
        _repo.GetByOwnerUidAsync(_ownerUid).Returns(contacts);

        var result = await _svc.GetByOwnerUidAsync(_ownerUid);
        Assert.Equal(2, result.Count());
    }

    // -------------------------------------------------------------------------
    // CreateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateAsync_CallsRepositoryAdd()
    {
        await _svc.CreateAsync(_ownerUid, "Alice Smith");
        await _repo.Received(1).AddAsync(Arg.Any<Contact>());
    }

    [Fact]
    public async Task CreateAsync_SetsCorrectFields()
    {
        var contact = await _svc.CreateAsync(_ownerUid, "Alice Smith", ["friend", "colleague"]);

        Assert.Equal(_ownerUid, contact.OwnerUserUid);
        Assert.Equal("Alice Smith", contact.Name);
        Assert.Equal(["friend", "colleague"], contact.Tags);
        Assert.NotEqual(Guid.Empty, contact.Uid);
    }

    [Fact]
    public async Task CreateAsync_UsesEmptyTagsList_WhenTagsNotProvided()
    {
        var contact = await _svc.CreateAsync(_ownerUid, "Bob");
        Assert.Empty(contact.Tags);
    }

    [Fact]
    public async Task CreateAsync_SetsTimestamps()
    {
        var before   = DateTime.UtcNow.AddSeconds(-1);
        var contact  = await _svc.CreateAsync(_ownerUid, "Charlie");
        var after    = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(contact.CreatedAt, before, after);
        Assert.InRange(contact.UpdatedAt, before, after);
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_SetsUpdatedAt_AndCallsRepository()
    {
        var contact = MakeContact();
        var before  = DateTime.UtcNow.AddSeconds(-1);
        await _svc.UpdateAsync(contact);
        var after   = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(contact.UpdatedAt, before, after);
        await _repo.Received(1).UpdateAsync(contact);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_CallsRepositoryDelete()
    {
        var uid = Guid.NewGuid();
        await _svc.DeleteAsync(uid);
        await _repo.Received(1).DeleteAsync(uid);
    }

    // -------------------------------------------------------------------------
    // AddEmailAddressAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddEmailAddressAsync_CallsRepositoryAdd()
    {
        var contactUid = Guid.NewGuid();
        await _svc.AddEmailAddressAsync(contactUid, "jane@example.com", EmailType.Personal);
        await _repo.Received(1).AddEmailAddressAsync(Arg.Any<EmailAddress>());
    }

    [Fact]
    public async Task AddEmailAddressAsync_SetsCorrectFields()
    {
        var contactUid = Guid.NewGuid();
        var result = await _svc.AddEmailAddressAsync(
            contactUid, "jane@example.com", EmailType.Work, isPrimary: true, tags: ["work"]);

        Assert.Equal(contactUid, result.ContactUid);
        Assert.Equal("jane@example.com", result.Email);
        Assert.Equal(EmailType.Work, result.Type);
        Assert.True(result.IsPrimary);
        Assert.Equal(["work"], result.Tags);
        Assert.NotEqual(Guid.Empty, result.Uid);
    }

    // -------------------------------------------------------------------------
    // UpdateEmailAddressAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateEmailAddressAsync_SetsUpdatedAt_AndCallsRepository()
    {
        var email  = new EmailAddress { Uid = Guid.NewGuid(), ContactUid = Guid.NewGuid(), Email = "a@b.com" };
        var before = DateTime.UtcNow.AddSeconds(-1);
        await _svc.UpdateEmailAddressAsync(email);
        var after  = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(email.UpdatedAt, before, after);
        await _repo.Received(1).UpdateEmailAddressAsync(email);
    }

    // -------------------------------------------------------------------------
    // DeleteEmailAddressAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteEmailAddressAsync_CallsRepositoryDelete()
    {
        var uid = Guid.NewGuid();
        await _svc.DeleteEmailAddressAsync(uid);
        await _repo.Received(1).DeleteEmailAddressAsync(uid);
    }

    // -------------------------------------------------------------------------
    // AddPhoneNumberAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddPhoneNumberAsync_CallsRepositoryAdd()
    {
        var contactUid = Guid.NewGuid();
        await _svc.AddPhoneNumberAsync(contactUid, "+1-555-0001", PhoneType.Mobile);
        await _repo.Received(1).AddPhoneNumberAsync(Arg.Any<PhoneNumber>());
    }

    [Fact]
    public async Task AddPhoneNumberAsync_SetsCorrectFields()
    {
        var contactUid = Guid.NewGuid();
        var result = await _svc.AddPhoneNumberAsync(
            contactUid, "+1-555-0001", PhoneType.Work, isPrimary: true, tags: ["main"]);

        Assert.Equal(contactUid, result.ContactUid);
        Assert.Equal("+1-555-0001", result.Number);
        Assert.Equal(PhoneType.Work, result.Type);
        Assert.True(result.IsPrimary);
        Assert.Equal(["main"], result.Tags);
    }

    // -------------------------------------------------------------------------
    // UpdatePhoneNumberAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdatePhoneNumberAsync_SetsUpdatedAt_AndCallsRepository()
    {
        var phone  = new PhoneNumber { Uid = Guid.NewGuid(), ContactUid = Guid.NewGuid(), Number = "+1-555-0002" };
        var before = DateTime.UtcNow.AddSeconds(-1);
        await _svc.UpdatePhoneNumberAsync(phone);
        var after  = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(phone.UpdatedAt, before, after);
        await _repo.Received(1).UpdatePhoneNumberAsync(phone);
    }

    // -------------------------------------------------------------------------
    // DeletePhoneNumberAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeletePhoneNumberAsync_CallsRepositoryDelete()
    {
        var uid = Guid.NewGuid();
        await _svc.DeletePhoneNumberAsync(uid);
        await _repo.Received(1).DeletePhoneNumberAsync(uid);
    }

    // -------------------------------------------------------------------------
    // AddAddressAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddAddressAsync_CallsRepositoryAdd()
    {
        var contactUid = Guid.NewGuid();
        var address = new ContactAddress
        {
            ContactUid = contactUid,
            Street = "123 Main St", City = "Springfield",
            State  = "IL", PostalCode = "62701", Country = "US",
        };
        await _svc.AddAddressAsync(contactUid, address);
        await _repo.Received(1).AddAddressAsync(address);
    }

    [Fact]
    public async Task AddAddressAsync_SetsContactUidAndTimestamps()
    {
        var contactUid = Guid.NewGuid();
        var address = new ContactAddress
        {
            ContactUid = contactUid,
            Street = "1 Test St", City = "CityA", State = "SA",
            PostalCode = "00001", Country = "US",
        };
        var before = DateTime.UtcNow.AddSeconds(-1);
        var result = await _svc.AddAddressAsync(contactUid, address);
        var after  = DateTime.UtcNow.AddSeconds(1);

        Assert.Equal(contactUid, result.ContactUid);
        Assert.NotEqual(Guid.Empty, result.Uid);
        Assert.InRange(result.CreatedAt, before, after);
        Assert.InRange(result.UpdatedAt, before, after);
    }

    // -------------------------------------------------------------------------
    // UpdateAddressAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAddressAsync_SetsUpdatedAt_AndCallsRepository()
    {
        var addr  = new ContactAddress
        {
            Uid = Guid.NewGuid(), ContactUid = Guid.NewGuid(),
            Street = "2 St", City = "B", State = "C", PostalCode = "00002", Country = "US",
        };
        var before = DateTime.UtcNow.AddSeconds(-1);
        await _svc.UpdateAddressAsync(addr);
        var after  = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(addr.UpdatedAt, before, after);
        await _repo.Received(1).UpdateAddressAsync(addr);
    }

    // -------------------------------------------------------------------------
    // DeleteAddressAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAddressAsync_CallsRepositoryDelete()
    {
        var uid = Guid.NewGuid();
        await _svc.DeleteAddressAsync(uid);
        await _repo.Received(1).DeleteAddressAsync(uid);
    }
}
