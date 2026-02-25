using AppSimple.Core.Auth;
using AppSimple.Core.Auth.Impl;
using AppSimple.Core.Enums;
using AppSimple.Core.Models;
using Microsoft.Extensions.Options;

namespace AppSimple.Core.Tests.Auth;

/// <summary>Tests for <see cref="JwtTokenService"/>.</summary>
public sealed class JwtTokenServiceTests
{
    private const string ValidSecret = "this-is-a-32-char-minimum-secret!!";

    private static JwtTokenService CreateService(
        string? secret = null,
        int expirationMinutes = 60)
    {
        var opts = new JwtOptions
        {
            Secret            = secret ?? ValidSecret,
            Issuer            = "TestIssuer",
            Audience          = "TestAudience",
            ExpirationMinutes = expirationMinutes
        };
        return new JwtTokenService(Options.Create(opts));
    }

    private static User MakeUser() => new()
    {
        Uid          = Guid.CreateVersion7(),
        Username     = "testuser",
        Email        = "test@example.com",
        PasswordHash = "$2a$11$hash",
        Role         = UserRole.User
    };

    // -------------------------------------------------------------------------
    // GenerateToken
    // -------------------------------------------------------------------------

    [Fact]
    public void GenerateToken_ReturnsNonEmptyString()
    {
        var svc   = CreateService();
        var token = svc.GenerateToken(MakeUser());
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GenerateToken_ReturnsDotSeparatedJwt()
    {
        var svc   = CreateService();
        var token = svc.GenerateToken(MakeUser());
        Assert.Equal(3, token.Split('.').Length);
    }

    [Fact]
    public void GenerateToken_DifferentCallsSameUser_ProduceDifferentTokens()
    {
        // jti claim is a fresh Guid each time
        var svc  = CreateService();
        var user = MakeUser();
        var t1   = svc.GenerateToken(user);
        var t2   = svc.GenerateToken(user);
        Assert.NotEqual(t1, t2);
    }

    // -------------------------------------------------------------------------
    // IsTokenValid
    // -------------------------------------------------------------------------

    [Fact]
    public void IsTokenValid_ValidToken_ReturnsTrue()
    {
        var svc   = CreateService();
        var token = svc.GenerateToken(MakeUser());
        Assert.True(svc.IsTokenValid(token));
    }

    [Fact]
    public void IsTokenValid_RandomString_ReturnsFalse()
    {
        var svc = CreateService();
        Assert.False(svc.IsTokenValid("not.a.token"));
    }

    [Fact]
    public void IsTokenValid_TokenSignedWithDifferentSecret_ReturnsFalse()
    {
        var svcA  = CreateService(secret: "secret-a-32-chars-minimumxxxxxxxxx");
        var svcB  = CreateService(secret: "secret-b-32-chars-minimumxxxxxxxxx");
        var token = svcA.GenerateToken(MakeUser());
        Assert.False(svcB.IsTokenValid(token));
    }

    [Fact]
    public void IsTokenValid_ExpiredToken_ReturnsFalse()
    {
        var svc   = CreateService(expirationMinutes: -1);  // expired 1 min ago
        var token = svc.GenerateToken(MakeUser());
        Assert.False(svc.IsTokenValid(token));
    }

    // -------------------------------------------------------------------------
    // GetUsernameFromToken
    // -------------------------------------------------------------------------

    [Fact]
    public void GetUsernameFromToken_ValidToken_ReturnsUsername()
    {
        var svc   = CreateService();
        var user  = MakeUser();
        var token = svc.GenerateToken(user);

        var username = svc.GetUsernameFromToken(token);

        Assert.Equal(user.Username, username);
    }

    [Fact]
    public void GetUsernameFromToken_InvalidToken_ReturnsNull()
    {
        var svc = CreateService();
        Assert.Null(svc.GetUsernameFromToken("invalid.token.here"));
    }

    [Fact]
    public void GetUsernameFromToken_EmptyString_ReturnsNull()
    {
        var svc = CreateService();
        Assert.Null(svc.GetUsernameFromToken(string.Empty));
    }
}
