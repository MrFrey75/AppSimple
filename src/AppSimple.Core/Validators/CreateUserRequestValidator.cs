using AppSimple.Core.Constants;
using AppSimple.Core.Models.Requests;
using FluentValidation;

namespace AppSimple.Core.Validators;

/// <summary>
/// Validates a <see cref="CreateUserRequest"/> before a new user is created.
/// </summary>
public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    /// <summary>Initializes all validation rules.</summary>
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(AppConstants.MaxUsernameLength)
                .WithMessage($"Username must not exceed {AppConstants.MaxUsernameLength} characters.")
            .Matches(@"^[a-zA-Z0-9_]+$")
                .WithMessage("Username may only contain letters, digits, and underscores.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(AppConstants.MaxEmailLength)
                .WithMessage($"Email must not exceed {AppConstants.MaxEmailLength} characters.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(AppConstants.MinPasswordLength)
                .WithMessage($"Password must be at least {AppConstants.MinPasswordLength} characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");

        RuleFor(x => x.FirstName)
            .MaximumLength(AppConstants.MaxNameLength)
                .WithMessage($"First name must not exceed {AppConstants.MaxNameLength} characters.")
            .When(x => x.FirstName is not null);

        RuleFor(x => x.LastName)
            .MaximumLength(AppConstants.MaxNameLength)
                .WithMessage($"Last name must not exceed {AppConstants.MaxNameLength} characters.")
            .When(x => x.LastName is not null);
    }
}
