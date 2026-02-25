using AppSimple.Core.Common.Exceptions;

namespace AppSimple.Core.Tests.Common;

/// <summary>Tests for all custom exception types.</summary>
public sealed class ExceptionTests
{
    // -------------------------------------------------------------------------
    // AppException
    // -------------------------------------------------------------------------

    [Fact]
    public void AppException_Message_IsSet()
    {
        var ex = new AppException("base error");
        Assert.Equal("base error", ex.Message);
    }

    [Fact]
    public void AppException_WithInner_SetsInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex    = new AppException("outer", inner);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void AppException_IsException()
    {
        Assert.True(typeof(AppException).IsSubclassOf(typeof(Exception)));
    }

    // -------------------------------------------------------------------------
    // EntityNotFoundException
    // -------------------------------------------------------------------------

    [Fact]
    public void EntityNotFoundException_SetsEntityType()
    {
        var ex = new EntityNotFoundException("User", Guid.NewGuid());
        Assert.Equal("User", ex.EntityType);
    }

    [Fact]
    public void EntityNotFoundException_SetsEntityId()
    {
        var id = Guid.NewGuid();
        var ex = new EntityNotFoundException("User", id);
        Assert.Equal(id, ex.EntityId);
    }

    [Fact]
    public void EntityNotFoundException_MessageContainsTypeAndId()
    {
        var id = Guid.NewGuid();
        var ex = new EntityNotFoundException("User", id);
        Assert.Contains("User", ex.Message);
        Assert.Contains(id.ToString(), ex.Message);
    }

    [Fact]
    public void EntityNotFoundException_IsAppException()
    {
        Assert.True(typeof(EntityNotFoundException).IsSubclassOf(typeof(AppException)));
    }

    // -------------------------------------------------------------------------
    // DuplicateEntityException
    // -------------------------------------------------------------------------

    [Fact]
    public void DuplicateEntityException_SetsField()
    {
        var ex = new DuplicateEntityException("Username", "alice");
        Assert.Equal("Username", ex.Field);
    }

    [Fact]
    public void DuplicateEntityException_SetsValue()
    {
        var ex = new DuplicateEntityException("Email", "a@b.com");
        Assert.Equal("a@b.com", ex.Value);
    }

    [Fact]
    public void DuplicateEntityException_MessageContainsFieldAndValue()
    {
        var ex = new DuplicateEntityException("Username", "alice");
        Assert.Contains("Username", ex.Message);
        Assert.Contains("alice", ex.Message);
    }

    // -------------------------------------------------------------------------
    // SystemEntityException
    // -------------------------------------------------------------------------

    [Fact]
    public void SystemEntityException_SetsEntityType()
    {
        var ex = new SystemEntityException("User");
        Assert.Equal("User", ex.EntityType);
    }

    [Fact]
    public void SystemEntityException_MessageMentionsEntityType()
    {
        var ex = new SystemEntityException("User");
        Assert.Contains("User", ex.Message);
    }

    // -------------------------------------------------------------------------
    // UnauthorizedException
    // -------------------------------------------------------------------------

    [Fact]
    public void UnauthorizedException_SetsMessage()
    {
        var ex = new UnauthorizedException("Access denied");
        Assert.Equal("Access denied", ex.Message);
    }

    [Fact]
    public void UnauthorizedException_IsAppException()
    {
        Assert.True(typeof(UnauthorizedException).IsSubclassOf(typeof(AppException)));
    }
}
