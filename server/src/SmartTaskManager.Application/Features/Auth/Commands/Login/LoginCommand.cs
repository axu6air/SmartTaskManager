using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password, string? IpAddress)
    : IRequest<Result<LoginResponse>>;
