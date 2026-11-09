using FluentValidation;

namespace SmartTaskManager.Application.Features.Auth.Commands.Logout;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(c => c.RefreshToken).NotEmpty().MaximumLength(256);
    }
}
