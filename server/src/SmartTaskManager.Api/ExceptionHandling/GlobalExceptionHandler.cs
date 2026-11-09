using Microsoft.AspNetCore.Diagnostics;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Domain.Exceptions;

namespace SmartTaskManager.Api.ExceptionHandling;

// Last line of defence: handles every exception the more specific handlers did not take.
// Expected failures are returned as Result and never reach this handler.
// Technical messages are never sent to the client, and the stack trace only in Development.
public sealed class GlobalExceptionHandler(
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger
) : IExceptionHandler
{
    private const string UnexpectedMessage = "An unexpected error occurred.";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var (statusCode, message) = Map(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path.Value);
        }
        else
        {
            logger.LogWarning("Request failed with {StatusCode}: {Reason}", statusCode, exception.Message);
        }

        await ExceptionResponseWriter.WriteAsync(httpContext, environment, exception, statusCode, message, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Message) Map(Exception exception)
    {
        if (exception is DomainException domain)
        {
            // Domain messages are written for users.
            return (StatusCodes.Status400BadRequest, domain.UiMessage ?? domain.Message);
        }

        if (exception is BadHttpRequestException badRequest)
        {
            return (badRequest.StatusCode, "The request is malformed.");
        }

        return (StatusCodes.Status500InternalServerError, UnexpectedMessage);
    }
}
