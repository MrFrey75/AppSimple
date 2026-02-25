using AppSimple.Core.Models.Requests;
using AppSimple.Core.Validators;
using FluentValidation.TestHelper;

namespace AppSimple.Core.Tests.Validators;

/// <summary>Tests for <see cref="ChangePasswordRequestValidator"/>.</summary>
public sealed class ChangePasswordRequestValidatorTests
{
    private readonly ChangePasswordRequestValidator _validator = new();

    private static ChangePasswordRequest Valid() => new()
    {
        CurrentPassword    = "OldSecure1",
        NewPassword        = "NewSecure1",
        ConfirmNewPassword = "NewSecure1"
    };

    [Fact]
    public void Valid_Request_PassesValidation()
    {
        _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CurrentPassword_Empty_FailsValidation()
    {
        var req = Valid(); req.CurrentPassword = "";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Fact]
    public void NewPassword_TooShort_FailsValidation()
    {
        var req = Valid(); req.NewPassword = "Ab1";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NewPassword_NoUppercase_FailsValidation()
    {
        var req = Valid(); req.NewPassword = "newpassword1";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NewPassword_NoDigit_FailsValidation()
    {
        var req = Valid(); req.NewPassword = "Newpassword";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NewPassword_SameAsCurrent_FailsValidation()
    {
        var req = Valid();
        req.NewPassword        = req.CurrentPassword;
        req.ConfirmNewPassword = req.CurrentPassword;
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void ConfirmNewPassword_Empty_FailsValidation()
    {
        var req = Valid(); req.ConfirmNewPassword = "";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.ConfirmNewPassword);
    }

    [Fact]
    public void ConfirmNewPassword_DoesNotMatchNew_FailsValidation()
    {
        var req = Valid(); req.ConfirmNewPassword = "Different1";
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.ConfirmNewPassword);
    }

    [Fact]
    public void ConfirmNewPassword_Null_SkipsConfirmValidation()
    {
        // When ConfirmNewPassword is null the When() guard suppresses all confirm checks
        var req = Valid();
        req.ConfirmNewPassword = null;
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.ConfirmNewPassword);
    }
}
