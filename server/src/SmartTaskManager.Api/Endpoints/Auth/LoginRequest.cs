using SmartTaskManager.Application.Common.Validation;
using FluentValidation;

namespace SmartTaskManager.Api.Endpoints.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(r => r.Email).NotEmpty().ValidEmail().MaximumLength(256);
        RuleFor(r => r.Password).NotEmpty().MaximumLength(128);
    }
}
