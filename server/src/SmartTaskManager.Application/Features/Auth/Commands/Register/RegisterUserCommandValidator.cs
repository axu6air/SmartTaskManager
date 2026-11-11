using SmartTaskManager.Application.Common.Validation;
using FluentValidation;

namespace SmartTaskManager.Application.Features.Auth.Commands.Register;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public const int MinPasswordLength = 8;

    // Lengths match dbo.Users; the password rules are the policy for every new account.
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.FullName).NotEmpty().MaximumLength(150);
        RuleFor(c => c.Email).NotEmpty().ValidEmail().MaximumLength(256);
        RuleFor(c => c.Password)
            .NotEmpty()
            .MinimumLength(MinPasswordLength)
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("The password needs an uppercase letter.")
            .Matches("[a-z]").WithMessage("The password needs a lowercase letter.")
            .Matches("[0-9]").WithMessage("The password needs a digit.");
        RuleFor(c => c.RoleId).NotEqual(Guid.Empty).When(c => c.RoleId.HasValue);
    }
}
