using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken, string? IpAddress) : IRequest<Result<bool>>;
