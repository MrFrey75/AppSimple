using AppSimple.Core.Models;

namespace AppSimple.Core.Tests.Models;

/// <summary>Tests for <see cref="BaseEntity"/> default values and behaviour.</summary>
public sealed class BaseEntityTests
{
    // Concrete subclass used for testing the abstract base
    private sealed class TestEntity : BaseEntity { }

    [Fact]
    public void Uid_DefaultsTo_NonEmptyGuid()
    {
        var entity = new TestEntity();
        Assert.NotEqual(Guid.Empty, entity.Uid);
    }

    [Fact]
    public void Uid_DefaultsTo_Version7Guid()
    {
        // Guid v7 encodes the creation time in the top 48 bits.
        // Variant bits 0b10xx at byte 8 and version nibble 0x7 at byte 6
        // are the canonical checks used by .NET's Guid.CreateVersion7().
        var entity = new TestEntity();
        var bytes = entity.Uid.ToByteArray();

        // Version nibble is stored in the high nibble of byte 7 (0-indexed) in RFC layout
        // .NET ToByteArray() uses a mixed-endian layout:
        //   bytes[7] holds the version & high-time bits in MS layout
        var version = (bytes[7] >> 4) & 0xF;
        Assert.Equal(7, version);
    }

    [Fact]
    public void Uid_IsUnique_AcrossInstances()
    {
        var a = new TestEntity();
        var b = new TestEntity();
        Assert.NotEqual(a.Uid, b.Uid);
    }

    [Fact]
    public void CreatedAt_DefaultsTo_MinValue()
    {
        var entity = new TestEntity();
        Assert.Equal(default, entity.CreatedAt);
    }

    [Fact]
    public void UpdatedAt_DefaultsTo_MinValue()
    {
        var entity = new TestEntity();
        Assert.Equal(default, entity.UpdatedAt);
    }

    [Fact]
    public void IsSystem_DefaultsTo_False()
    {
        var entity = new TestEntity();
        Assert.False(entity.IsSystem);
    }

    [Fact]
    public void Properties_CanBeSetAndRead()
    {
        var now = DateTime.UtcNow;
        var uid = Guid.NewGuid();
        var entity = new TestEntity
        {
            Uid = uid,
            CreatedAt = now,
            UpdatedAt = now,
            IsSystem = true
        };

        Assert.Equal(uid, entity.Uid);
        Assert.Equal(now, entity.CreatedAt);
        Assert.Equal(now, entity.UpdatedAt);
        Assert.True(entity.IsSystem);
    }
}
