using Microsoft.AspNetCore.Diagnostics;

namespace SmartTaskManager.Api.ExceptionHandling;

// InvalidOperationException means the server is in a state it should never reach
// (for example no authenticated user inside an authorized endpoint, or a missing seed role).
// It is a server fault, so the message stays in the log and the client gets a generic 500.
public sealed class InvalidOperationExceptionHandler(
    IHostEnvironment environment,
    ILogger<InvalidOperationExceptionHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is not InvalidOperationException)
        {
            return false;
        }

        logger.LogError(exception, "Invalid server state for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path.Value);
        await ExceptionResponseWriter.WriteAsync(
            httpContext,
            environment,
            exception,
            StatusCodes.Status500InternalServerError,
            "The server could not process the request in its current state.",
            cancellationToken
        );

        return true;
    }
}
