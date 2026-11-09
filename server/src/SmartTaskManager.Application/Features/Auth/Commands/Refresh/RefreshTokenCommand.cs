using MediatR;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Features.Auth.Commands.Login;

namespace SmartTaskManager.Application.Features.Auth.Commands.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken, string? IpAddress)
    : IRequest<Result<LoginResponse>>;
