using AppSimple.Core.Services;

namespace AppSimple.Core.Tests.Services;

/// <summary>Tests for the <see cref="AuthResult"/> factory methods and properties.</summary>
public sealed class AuthResultTests
{
    // -------------------------------------------------------------------------
    // Success factory
    // -------------------------------------------------------------------------

    [Fact]
    public void Success_Sets_Succeeded_True()
    {
        var result = AuthResult.Success("token123");
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Success_Sets_Token()
    {
        var result = AuthResult.Success("my.jwt.token");
        Assert.Equal("my.jwt.token", result.Token);
    }

    [Fact]
    public void Success_Sets_Message()
    {
        var result = AuthResult.Success("token");
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
    }

    [Fact]
    public void Success_Message_ContainsSuccessfulKeyword()
    {
        var result = AuthResult.Success("token");
        Assert.Contains("successful", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    // -------------------------------------------------------------------------
    // Failure factory
    // -------------------------------------------------------------------------

    [Fact]
    public void Failure_Sets_Succeeded_False()
    {
        var result = AuthResult.Failure("Bad credentials");
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Failure_Token_IsNull()
    {
        var result = AuthResult.Failure("Bad credentials");
        Assert.Null(result.Token);
    }

    [Fact]
    public void Failure_Sets_Message()
    {
        var result = AuthResult.Failure("Invalid username or password");
        Assert.Equal("Invalid username or password", result.Message);
    }

    [Theory]
    [InlineData("User not found")]
    [InlineData("Account disabled")]
    [InlineData("Password expired")]
    public void Failure_PreservesArbitraryMessage(string message)
    {
        var result = AuthResult.Failure(message);
        Assert.Equal(message, result.Message);
    }

    // -------------------------------------------------------------------------
    // Property mutability
    // -------------------------------------------------------------------------

    [Fact]
    public void Token_CanBeSetDirectly()
    {
        var result = new AuthResult { Succeeded = false, Message = "test" };
        result.Token = "override";
        Assert.Equal("override", result.Token);
    }
}
