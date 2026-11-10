using System.ComponentModel.DataAnnotations;

namespace SmartTaskManager.Api.RateLimiting;

// Fixed windows, counted per user, or per IP address for anonymous requests.
public sealed class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    [Range(1, 3600)]
    public int WindowSeconds { get; init; } = 60;

    // Every endpoint, per window.
    [Range(1, 100_000)]
    public int GlobalPermitLimit { get; init; } = 100;

    // Auth endpoints, per window. Low to slow down password guessing.
    [Range(1, 1000)]
    public int AuthPermitLimit { get; init; } = 10;

    // AI endpoints, per window. Low because each call costs model quota.
    [Range(1, 1000)]
    public int AiPermitLimit { get; init; } = 10;
}
