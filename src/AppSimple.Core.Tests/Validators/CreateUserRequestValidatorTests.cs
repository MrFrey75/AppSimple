using AppSimple.Core.Models.Requests;
using AppSimple.Core.Validators;
using FluentValidation.TestHelper;

namespace AppSimple.Core.Tests.Validators;

/// <summary>Tests for <see cref="CreateUserRequestValidator"/>.</summary>
public sealed class CreateUserRequestValidatorTests
{
    private readonly CreateUserRequestValidator _validator = new();

    private static CreateUserRequest Valid() => new()
    {
        Username = "alice_99",
        Email    = "alice@example.com",
        Password = "SecurePass1"
    };

    // -------------------------------------------------------------------------
    // Username
    // -------------------------------------------------------------------------

    [Fact]
    public void Valid_Request_PassesValidation()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Username_Empty_FailsValidation()
    {
        var req = Valid(); req.Username = "";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Username_TooLong_FailsValidation()
    {
        var req = Valid(); req.Username = new string('a', 51);
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("has space")]
    [InlineData("has-dash")]
    [InlineData("has@symbol")]
    [InlineData("has.dot")]
    public void Username_InvalidChars_FailsValidation(string username)
    {
        var req = Valid(); req.Username = username;
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("alice")]
    [InlineData("Alice99")]
    [InlineData("alice_99")]
    [InlineData("ALICE")]
    public void Username_ValidFormats_PassesValidation(string username)
    {
        var req = Valid(); req.Username = username;
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    // -------------------------------------------------------------------------
    // Email
    // -------------------------------------------------------------------------

    [Fact]
    public void Email_Empty_FailsValidation()
    {
        var req = Valid(); req.Email = "";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    public void Email_InvalidFormat_FailsValidation(string email)
    {
        var req = Valid(); req.Email = email;
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("user+tag@domain.org")]
    public void Email_ValidFormat_PassesValidation(string email)
    {
        var req = Valid(); req.Email = email;
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    // -------------------------------------------------------------------------
    // Password
    // -------------------------------------------------------------------------

    [Fact]
    public void Password_Empty_FailsValidation()
    {
        var req = Valid(); req.Password = "";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_TooShort_FailsValidation()
    {
        var req = Valid(); req.Password = "Ab1";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_NoUppercase_FailsValidation()
    {
        var req = Valid(); req.Password = "alllower1";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_NoLowercase_FailsValidation()
    {
        var req = Valid(); req.Password = "ALLUPPER1";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_NoDigit_FailsValidation()
    {
        var req = Valid(); req.Password = "NoDigitHere";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Password);
    }

    // -------------------------------------------------------------------------
    // Optional name fields
    // -------------------------------------------------------------------------

    [Fact]
    public void FirstName_Null_PassesValidation()
    {
        var req = Valid(); req.FirstName = null;
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void FirstName_TooLong_FailsValidation()
    {
        var req = Valid(); req.FirstName = new string('a', 101);
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.FirstName);
    }
}
