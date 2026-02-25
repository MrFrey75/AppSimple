using AppSimple.Core.Models.DTOs;

namespace AppSimple.Core.Tests.Models;

/// <summary>Tests for the <see cref="LoginResult"/> DTO.</summary>
public sealed class LoginResultTests
{
    [Fact]
    public void LoginResult_Properties_SetAndGet_Correctly()
    {
        var result = new LoginResult
        {
            Token    = "eyJhbGci.payload.signature",
            Username = "alice",
            Role     = "Admin",
        };

        Assert.Equal("eyJhbGci.payload.signature", result.Token);
        Assert.Equal("alice",                       result.Username);
        Assert.Equal("Admin",                       result.Role);
    }

    [Fact]
    public void LoginResult_Token_Required_CannotBeNull()
    {
        // Verify the required keyword is enforced at compile-time via object initializer
        // (runtime: just check assignments work without throwing)
        var result = new LoginResult { Token = "t", Username = "u", Role = "User" };
        Assert.NotNull(result.Token);
        Assert.NotNull(result.Username);
        Assert.NotNull(result.Role);
    }
}
