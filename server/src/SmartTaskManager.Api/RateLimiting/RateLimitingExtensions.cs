using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using SmartTaskManager.Api.Common;

namespace SmartTaskManager.Api.RateLimiting;

public static class RateLimitPolicies
{
    public const string Auth = "auth";
    public const string Ai = "ai";
}

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services
            .AddOptions<RateLimitingSettings>()
            .BindConfiguration(RateLimitingSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddRateLimiter(_ => { });

        // Configured through options so the validated RateLimitingSettings is the single source of the limits.
        services
            .AddOptions<RateLimiterOptions>()
            .Configure<IOptions<RateLimitingSettings>>(
                (options, settingsOptions) =>
                {
                    var settings = settingsOptions.Value;

                    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                    options.OnRejected = WriteRejectionAsync;

                    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
                        FixedWindow(http, settings.GlobalPermitLimit, settings.WindowSeconds)
                    );
                    options.AddPolicy(
                        RateLimitPolicies.Auth,
                        http => FixedWindow(http, settings.AuthPermitLimit, settings.WindowSeconds)
                    );
                    options.AddPolicy(
                        RateLimitPolicies.Ai,
                        http => FixedWindow(http, settings.AiPermitLimit, settings.WindowSeconds)
                    );
                }
            );

        return services;
    }

    // Authenticated requests are counted per user, so users behind one NAT do not share a limit.
    // Anonymous requests (login, refresh) are counted per IP address.
    public static string GetPartitionKey(HttpContext http) =>
        http.User.GetUserId() is { } userId
            ? $"user:{userId}"
            : $"ip:{http.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

    private static RateLimitPartition<string> FixedWindow(HttpContext http, int permitLimit, int windowSeconds) =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetPartitionKey(http),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueLimit = 0,
            }
        );

    private static async ValueTask WriteRejectionAsync(OnRejectedContext context, CancellationToken ct)
    {
        var http = context.HttpContext;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            http.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(
                CultureInfo.InvariantCulture
            );
        }

        var body = new ApiResponse<object>(
            false,
            null,
            "Too many requests. Try again later.",
            null,
            http.TraceIdentifier
        );

        await http.Response.WriteAsJsonAsync(body, ct);
    }
}
