using System.Buffers.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Domain.Models.Identity;

namespace SmartTaskManager.Infrastructure.Security;

internal sealed class TokenService(IOptions<JwtConfiguration> options, TimeProvider timeProvider) : ITokenService
{
    private readonly JwtConfiguration _jwt = options.Value;

    public TimeSpan AccessTokenLifetime => TimeSpan.FromMinutes(_jwt.TokenExpiry);

    public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(_jwt.RefreshTokenExpiry);

    public string CreateAccessToken(User user)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        // Permissions are resolved server-side per request, so the token carries identity only.
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            ]),
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = now.Add(AccessTokenLifetime),
            SigningCredentials = _jwt.SigningCredentials,
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    public string CreateRefreshToken() => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(64));

    // Hex SHA-256, 64 characters, matching RefreshTokens.TokenHash CHAR(64).
    public string HashRefreshToken(string refreshToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
}
