using Microsoft.AspNetCore.Diagnostics;
using SmartTaskManager.Infrastructure.Errors;
using SmartTaskManager.Infrastructure.Exceptions;

namespace SmartTaskManager.Api.ExceptionHandling;

// Maps database failures wrapped by the repositories. Only the UiMessage reaches the client,
// because the technical message can contain SQL details.
public sealed class RepositoryExceptionHandler(
    IHostEnvironment environment,
    ILogger<RepositoryExceptionHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is not RepositoryException repository)
        {
            return false;
        }

        var (statusCode, message) = Map(repository);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Database failure ({ErrorType}) for {Method} {Path}", repository.ErrorType, httpContext.Request.Method, httpContext.Request.Path.Value);
        }
        else
        {
            logger.LogWarning("Database conflict ({ErrorType}) for {Method} {Path}", repository.ErrorType, httpContext.Request.Method, httpContext.Request.Path.Value);
        }

        await ExceptionResponseWriter.WriteAsync(httpContext, environment, exception, statusCode, message, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Message) Map(RepositoryException exception)
    {
        var type = exception.ErrorType;

        if (type == RepositoryErrorType.ConcurrencyConflict || type == RepositoryErrorType.Deadlock)
        {
            return (
                StatusCodes.Status409Conflict,
                exception.UiMessage ?? "The data was changed by someone else. Reload and try again."
            );
        }

        if (
            type == RepositoryErrorType.CommandTimeout
            || type == RepositoryErrorType.ConnectionTimeout
            || type == RepositoryErrorType.RetryPolicyTimeout
            || type == RepositoryErrorType.RetryExhausted
            || type == RepositoryErrorType.Timeout
        )
        {
            return (
                StatusCodes.Status503ServiceUnavailable,
                exception.UiMessage ?? "The service is busy. Try again in a moment."
            );
        }

        return (
            StatusCodes.Status500InternalServerError,
            exception.UiMessage ?? "A database error occurred."
        );
    }
}
