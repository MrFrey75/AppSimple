using AppSimple.Core.Models.Requests;
using AppSimple.Core.Validators;
using FluentValidation.TestHelper;

namespace AppSimple.Core.Tests.Validators;

/// <summary>Tests for <see cref="UpdateUserRequestValidator"/>.</summary>
public sealed class UpdateUserRequestValidatorTests
{
    private readonly UpdateUserRequestValidator _validator = new();

    [Fact]
    public void AllNullFields_PassesValidation()
    {
        var result = _validator.TestValidate(new UpdateUserRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void FirstName_TooLong_FailsValidation()
    {
        var req = new UpdateUserRequest { FirstName = new string('a', 101) };
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void LastName_TooLong_FailsValidation()
    {
        var req = new UpdateUserRequest { LastName = new string('b', 101) };
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void PhoneNumber_TooLong_FailsValidation()
    {
        var req = new UpdateUserRequest { PhoneNumber = new string('1', 31) };
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [InlineData("+1-555-123-4567")]
    [InlineData("(555) 123 4567")]
    [InlineData("+44 20 7946 0958")]
    public void PhoneNumber_ValidFormats_PassesValidation(string phone)
    {
        var req = new UpdateUserRequest { PhoneNumber = phone };
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void PhoneNumber_InvalidChars_FailsValidation()
    {
        var req = new UpdateUserRequest { PhoneNumber = "abc_not_a_phone" };
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void DateOfBirth_FutureDate_FailsValidation()
    {
        var req = new UpdateUserRequest { DateOfBirth = DateTime.UtcNow.AddYears(1) };
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void DateOfBirth_Before1900_FailsValidation()
    {
        var req = new UpdateUserRequest { DateOfBirth = new DateTime(1899, 12, 31) };
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void DateOfBirth_ValidDate_PassesValidation()
    {
        var req = new UpdateUserRequest { DateOfBirth = new DateTime(1990, 6, 15) };
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Bio_TooLong_FailsValidation()
    {
        var req = new UpdateUserRequest { Bio = new string('x', 501) };
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Bio);
    }

    [Fact]
    public void Bio_WithinLimit_PassesValidation()
    {
        var req = new UpdateUserRequest { Bio = "Short bio." };
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.Bio);
    }
}
