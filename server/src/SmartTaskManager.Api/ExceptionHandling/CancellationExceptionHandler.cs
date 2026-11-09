using Microsoft.AspNetCore.Diagnostics;

namespace SmartTaskManager.Api.ExceptionHandling;

// Handles OperationCanceledException (and TaskCanceledException, which derives from it)
// and any failure that happens after the client has disconnected.
public sealed class CancellationExceptionHandler(
    IHostEnvironment environment,
    ILogger<CancellationExceptionHandler> logger
) : IExceptionHandler
{
    // Nginx's "client closed request". Nobody reads this response, it only keeps the request log accurate.
    private const int ClientClosedRequest = 499;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation("The client cancelled {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path.Value);
            httpContext.Response.StatusCode = ClientClosedRequest;
            return true;
        }

        if (exception is not OperationCanceledException)
        {
            return false;
        }

        // Cancelled while the client is still waiting, for example an internal timeout.
        logger.LogWarning(exception, "An operation timed out for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path.Value);
        await ExceptionResponseWriter.WriteAsync(
            httpContext,
            environment,
            exception,
            StatusCodes.Status504GatewayTimeout,
            "The operation took too long. Try again.",
            cancellationToken
        );

        return true;
    }
}
