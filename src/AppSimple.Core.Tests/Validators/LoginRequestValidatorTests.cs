using AppSimple.Core.Models.Requests;
using AppSimple.Core.Validators;
using FluentValidation.TestHelper;

namespace AppSimple.Core.Tests.Validators;

/// <summary>Tests for <see cref="LoginRequestValidator"/>.</summary>
public sealed class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _sut = new();

    [Fact]
    public void Username_WhenEmpty_ShouldFail()
    {
        var result = _sut.TestValidate(new LoginRequest { Username = "", Password = "pass" });
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Username_WhenWhitespace_ShouldFail()
    {
        var result = _sut.TestValidate(new LoginRequest { Username = "   ", Password = "pass" });
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Password_WhenEmpty_ShouldFail()
    {
        var result = _sut.TestValidate(new LoginRequest { Username = "user", Password = "" });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_WhenWhitespace_ShouldFail()
    {
        var result = _sut.TestValidate(new LoginRequest { Username = "user", Password = "   " });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ValidRequest_ShouldPass()
    {
        var result = _sut.TestValidate(new LoginRequest { Username = "admin", Password = "Admin123!" });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UsernameError_ContainsHelpfulMessage()
    {
        var result = _sut.TestValidate(new LoginRequest { Username = "", Password = "pass" });
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username is required.");
    }

    [Fact]
    public void PasswordError_ContainsHelpfulMessage()
    {
        var result = _sut.TestValidate(new LoginRequest { Username = "user", Password = "" });
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password is required.");
    }
}
