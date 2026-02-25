using AppSimple.Core.Constants;
using AppSimple.Core.Models.Requests;
using FluentValidation;

namespace AppSimple.Core.Validators;

/// <summary>
/// Validates a <see cref="ChangePasswordRequest"/> before a user's password is changed.
/// </summary>
public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    /// <summary>Initializes all validation rules.</summary>
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(AppConstants.MinPasswordLength)
                .WithMessage($"New password must be at least {AppConstants.MinPasswordLength} characters.")
            .Matches("[A-Z]").WithMessage("New password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("New password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("New password must contain at least one digit.")
            .NotEqual(x => x.CurrentPassword)
                .WithMessage("New password must differ from the current password.");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Password confirmation is required.")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match.")
            .When(x => x.ConfirmNewPassword is not null);
    }
}
