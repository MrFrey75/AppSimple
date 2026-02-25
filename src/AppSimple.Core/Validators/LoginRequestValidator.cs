using AppSimple.Core.Models.Requests;
using FluentValidation;

namespace AppSimple.Core.Validators;

/// <summary>
/// Validates a <see cref="LoginRequest"/> before authentication is attempted.
/// </summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    /// <summary>Initializes all validation rules.</summary>
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
