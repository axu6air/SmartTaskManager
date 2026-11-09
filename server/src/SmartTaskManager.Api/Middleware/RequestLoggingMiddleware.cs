namespace SmartTaskManager.Api.Middleware;

// Logs one line per request: who made it, when, and how it ended. Runs after UseAuthentication so the user is known.
// Only the path is logged, never the query string, headers or body, so tokens and passwords stay out of the logs.
public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    TimeProvider timeProvider,
    ILogger<RequestLoggingMiddleware> logger
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var requestedAt = timeProvider.GetUtcNow();
        var started = timeProvider.GetTimestamp();
        var statusCode = StatusCodes.Status500InternalServerError;

        try
        {
            await next(context);
            statusCode = context.Response.StatusCode;
        }
        finally
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "{Method} {Path} by {UserId} from {RemoteIp} at {RequestedAt:o} returned {StatusCode} in {ElapsedMs} ms",
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.User.GetUserId()?.ToString() ?? "anonymous",
                    context.Connection.RemoteIpAddress,
                    requestedAt,
                    statusCode,
                    (long)timeProvider.GetElapsedTime(started).TotalMilliseconds
                );
            }
        }
    }
}

public static class RequestLoggingExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app) =>
        app.UseMiddleware<RequestLoggingMiddleware>();
}
