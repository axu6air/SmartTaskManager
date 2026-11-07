using SmartTaskManager.Domain.Models.Identity;

namespace SmartTaskManager.Application.Common.Abstractions;

public interface ITokenService
{
    TimeSpan AccessTokenLifetime { get; }

    TimeSpan RefreshTokenLifetime { get; }

    string CreateAccessToken(User user);

    // A new random token. Only its hash (HashRefreshToken) is ever stored.
    string CreateRefreshToken();

    string HashRefreshToken(string refreshToken);
}
