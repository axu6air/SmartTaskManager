using SmartTaskManager.Application.Common.Validation;
using FluentValidation;

namespace SmartTaskManager.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().ValidEmail().MaximumLength(256);

        // No complexity rules here: those belong to registration and would only leak policy at login.
        RuleFor(c => c.Password).NotEmpty().MaximumLength(128);
    }
}
