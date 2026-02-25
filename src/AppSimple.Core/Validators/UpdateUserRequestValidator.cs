using AppSimple.Core.Constants;
using AppSimple.Core.Models.Requests;
using FluentValidation;

namespace AppSimple.Core.Validators;

/// <summary>
/// Validates an <see cref="UpdateUserRequest"/> before a user's profile is updated.
/// </summary>
public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    /// <summary>Initializes all validation rules.</summary>
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(AppConstants.MaxNameLength)
                .WithMessage($"First name must not exceed {AppConstants.MaxNameLength} characters.")
            .When(x => x.FirstName is not null);

        RuleFor(x => x.LastName)
            .MaximumLength(AppConstants.MaxNameLength)
                .WithMessage($"Last name must not exceed {AppConstants.MaxNameLength} characters.")
            .When(x => x.LastName is not null);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(AppConstants.MaxPhoneLength)
                .WithMessage($"Phone number must not exceed {AppConstants.MaxPhoneLength} characters.")
            .Matches(@"^[+\d\s\-().]+$")
                .WithMessage("Phone number contains invalid characters.")
            .When(x => x.PhoneNumber is not null);

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past.")
            .GreaterThan(new DateTime(1900, 1, 1)).WithMessage("Date of birth is not realistic.")
            .When(x => x.DateOfBirth is not null);

        RuleFor(x => x.Bio)
            .MaximumLength(AppConstants.MaxBioLength)
                .WithMessage($"Bio must not exceed {AppConstants.MaxBioLength} characters.")
            .When(x => x.Bio is not null);
    }
}
