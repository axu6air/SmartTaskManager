using System.ComponentModel.DataAnnotations;
using Microsoft.IdentityModel.Tokens;

namespace SmartTaskManager.Infrastructure.Security;

public sealed class JwtConfiguration
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    // Minutes.
    [Range(1, 1440)]
    public int TokenExpiry { get; set; } = 15;

    // Days.
    [Range(1, 90)]
    public int RefreshTokenExpiry { get; set; } = 7;

    // HMAC-SHA256 needs a key of at least 256 bits.
    [Required, MinLength(32)]
    public string SecretKey { get; set; } = string.Empty;

    // Derived from SecretKey by JwtConfigurationSetup; never read from configuration.
    public SigningCredentials? SigningCredentials { get; set; }
}
