using Microsoft.AspNetCore.Diagnostics;
using SmartTaskManager.Application.Common.Exceptions;

namespace SmartTaskManager.Api.ExceptionHandling;

// Maps AI service failures. Only the UiMessage reaches the client; the inner exception stays in the log.
public sealed class AiServiceExceptionHandler(
    IHostEnvironment environment,
    ILogger<AiServiceExceptionHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is not AiServiceException ai)
        {
            return false;
        }

        // A failing external service is not our bug, so it is a warning, not an error.
        logger.LogWarning(exception, "AI service failure ({ErrorType}) for {Method} {Path}", ai.ErrorType, httpContext.Request.Method, httpContext.Request.Path.Value);

        await ExceptionResponseWriter.WriteAsync(
            httpContext,
            environment,
            exception,
            ToStatusCode(ai.ErrorType),
            ai.UiMessage ?? "The AI service failed. Try again later.",
            cancellationToken
        );

        return true;
    }

    private static int ToStatusCode(AiErrorType type)
    {
        if (type == AiErrorType.Timeout)
        {
            return StatusCodes.Status504GatewayTimeout;
        }

        if (type == AiErrorType.InvalidResponse)
        {
            return StatusCodes.Status502BadGateway;
        }

        return StatusCodes.Status503ServiceUnavailable;
    }
}
