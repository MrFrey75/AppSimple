using AppSimple.Core.Models.Requests;

namespace AppSimple.Core.Tests.Models;

/// <summary>Tests for the <see cref="LoginRequest"/> model.</summary>
public sealed class LoginRequestTests
{
    [Fact]
    public void LoginRequest_Properties_SetAndGet_Correctly()
    {
        var req = new LoginRequest { Username = "alice", Password = "Secret1!" };

        Assert.Equal("alice",    req.Username);
        Assert.Equal("Secret1!", req.Password);
    }

    [Fact]
    public void LoginRequest_Username_AcceptsAnyNonNullString()
    {
        var req = new LoginRequest { Username = "user_123", Password = "p" };
        Assert.Equal("user_123", req.Username);
    }

    [Fact]
    public void LoginRequest_Password_IsStoredVerbatim()
    {
        var req = new LoginRequest { Username = "u", Password = "P@ssw0rd!!" };
        Assert.Equal("P@ssw0rd!!", req.Password);
    }
}
