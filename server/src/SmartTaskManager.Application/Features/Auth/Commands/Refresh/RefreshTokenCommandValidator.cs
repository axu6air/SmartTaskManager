using FluentValidation;

namespace SmartTaskManager.Application.Features.Auth.Commands.Refresh;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(c => c.RefreshToken).NotEmpty().MaximumLength(256);
    }
}
