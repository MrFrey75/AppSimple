using AppSimple.Core.Auth.Impl;

namespace AppSimple.Core.Tests.Auth;

/// <summary>Tests for <see cref="BcryptPasswordHasher"/>.</summary>
public sealed class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_ReturnsNonEmptyString()
    {
        var hash = _hasher.Hash("SecurePass1");
        Assert.False(string.IsNullOrWhiteSpace(hash));
    }

    [Fact]
    public void Hash_DifferentCallsSamePlaintext_ProduceDifferentHashes()
    {
        // BCrypt uses a random salt per call
        var h1 = _hasher.Hash("SecurePass1");
        var h2 = _hasher.Hash("SecurePass1");
        Assert.NotEqual(h1, h2);
    }

    [Fact]
    public void Hash_ResultStartsWithBcryptPrefix()
    {
        var hash = _hasher.Hash("SecurePass1");
        Assert.StartsWith("$2", hash);
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("MyPassword1");
        Assert.True(_hasher.Verify("MyPassword1", hash));
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("MyPassword1");
        Assert.False(_hasher.Verify("WrongPassword1", hash));
    }

    [Fact]
    public void Verify_EmptyPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("MyPassword1");
        Assert.False(_hasher.Verify("", hash));
    }

    [Fact]
    public void Verify_CaseSensitive()
    {
        var hash = _hasher.Hash("MyPassword1");
        Assert.False(_hasher.Verify("mypassword1", hash));
    }

    [Theory]
    [InlineData("Short1")]
    [InlineData("ALLUPPERCASE1")]
    [InlineData("alllowercase1")]
    [InlineData("NoDigitsHere")]
    public void Hash_AndVerify_WorkForArbitraryPasswords(string password)
    {
        var hash = _hasher.Hash(password);
        Assert.True(_hasher.Verify(password, hash));
    }
}
