using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Domain.Models.Identity;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Auth.Commands.Logout;

// Holding the refresh token is the credential, so logout works even after the access token expired.
// Always succeeds, so a caller cannot probe which tokens exist.
public sealed class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokens,
    ITokenService tokenService,
    TimeProvider timeProvider
) : IRequestHandler<LogoutCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow();
        var hash = tokenService.HashRefreshToken(request.RefreshToken);
        var token = await refreshTokens.FindAsync(t => t.TokenHash == hash, tracking: true, ct: ct);

        if (token is not null && token.IsActive(now))
        {
            token.Revoke(now, RefreshTokenRevokeReasons.Logout, request.IpAddress);
            await refreshTokens.SaveChangesAsync(ct);
        }

        return Result<bool>.Success(true);
    }
}
