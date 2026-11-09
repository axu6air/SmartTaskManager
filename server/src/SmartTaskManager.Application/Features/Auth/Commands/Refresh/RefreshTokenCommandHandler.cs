using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Features.Auth.Commands.Login;
using SmartTaskManager.Domain.Models.Identity;
using SmartTaskManager.Domain.Repositories;


namespace SmartTaskManager.Application.Features.Auth.Commands.Refresh;

// Rotates the refresh token on every use. Reusing a revoked token revokes its whole family,
// because it means the token was stolen or replayed.
public sealed class RefreshTokenCommandHandler(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    ITokenService tokenService,
    TimeProvider timeProvider
) : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private static readonly Error InvalidToken = new(ErrorType.Unauthorized, "The refresh token is not valid.");

    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow();
        var hash = tokenService.HashRefreshToken(request.RefreshToken);
        var current = await refreshTokens.FindAsync(t => t.TokenHash == hash, tracking: true, ct: ct);

        if (current is null)
        {
            return Result<LoginResponse>.Failure(InvalidToken);
        }

        if (current.IsRevoked)
        {
            await RevokeFamilyAsync(current.FamilyId, now, request.IpAddress, ct);
            return Result<LoginResponse>.Failure(InvalidToken);
        }

        if (!current.IsActive(now))
        {
            return Result<LoginResponse>.Failure(InvalidToken);
        }

        var user = await users.FindAsync(current.UserId, ct: ct);
        if (user is null or { IsActive: false } || user.IsLockedOut(now))
        {
            current.Revoke(now, RefreshTokenRevokeReasons.Logout, request.IpAddress);
            await refreshTokens.SaveChangesAsync(ct);
            return Result<LoginResponse>.Failure(InvalidToken);
        }

        var newToken = tokenService.CreateRefreshToken();
        var replacement = RefreshToken.Create(
            user.Id,
            tokenService.HashRefreshToken(newToken),
            current.FamilyId,
            now.Add(tokenService.RefreshTokenLifetime),
            request.IpAddress
        );

        // Staged first so the replacement has its key before the old token points at it; one save keeps both atomic.
        await refreshTokens.CreateAsync(replacement, saveChanges: false, ct);
        current.Revoke(now, RefreshTokenRevokeReasons.Rotated, request.IpAddress, replacement.Id);
        await refreshTokens.SaveChangesAsync(ct);

        return Result<LoginResponse>.Success(
            new LoginResponse(
                tokenService.CreateAccessToken(user),
                newToken,
                now.Add(tokenService.AccessTokenLifetime)
            )
        );
    }

    private async Task RevokeFamilyAsync(Guid familyId, DateTimeOffset now, string? ip, CancellationToken ct)
    {
        var active = await refreshTokens.FindAllAsync(
            t => t.FamilyId == familyId && t.RevokedAt == null,
            tracking: true,
            ct: ct
        );

        foreach (var token in active)
        {
            token.Revoke(now, RefreshTokenRevokeReasons.ReuseDetected, ip);
        }

        await refreshTokens.SaveChangesAsync(ct);
    }
}
