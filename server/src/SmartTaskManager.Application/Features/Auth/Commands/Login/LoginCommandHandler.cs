using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Domain.Models.Identity;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    TimeProvider timeProvider
) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private const int MaxFailedAttempts = 5;

    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    // One message for every failure, so a caller cannot tell a wrong password from an unknown or locked account.
    private static readonly Error InvalidCredentials = new(
        ErrorType.Unauthorized,
        "Invalid email or password."
    );

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken
    )
    {
        var now = timeProvider.GetUtcNow();
        var normalizedEmail = User.NormalizeEmail(request.Email);
        var user = await userRepository.FindAsync(
            u => u.NormalizedEmail == normalizedEmail,
            ct: cancellationToken
        );

        if (user is null or { IsActive: false } || user.IsLockedOut(now))
        {
            return Result<LoginResponse>.Failure(InvalidCredentials);
        }

        if (!passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            user.RecordFailedLogin(now, MaxFailedAttempts, LockoutDuration);
            await userRepository.UpdateAsync(user, cancellationToken);

            return Result<LoginResponse>.Failure(InvalidCredentials);
        }

        user.RecordSuccessfulLogin(now);
        await userRepository.UpdateAsync(user, cancellationToken);

        var refreshToken = tokenService.CreateRefreshToken();
        await refreshTokens.CreateAsync(
            RefreshToken.Create(
                user.Id,
                tokenService.HashRefreshToken(refreshToken),
                familyId: Guid.NewGuid(),
                now.Add(tokenService.RefreshTokenLifetime),
                request.IpAddress
            ),
            cancellationToken
        );

        return Result<LoginResponse>.Success(
            new LoginResponse(
                tokenService.CreateAccessToken(user),
                refreshToken,
                now.Add(tokenService.AccessTokenLifetime)
            )
        );
    }
}
