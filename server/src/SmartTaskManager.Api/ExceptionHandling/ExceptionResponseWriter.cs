using SmartTaskManager.Api.Common;

namespace SmartTaskManager.Api.ExceptionHandling;

// The one place an exception becomes an ApiResponse, so every handler answers in the same shape.
// The stack trace is only included in Development.
internal static class ExceptionResponseWriter
{
    public static async ValueTask WriteAsync(
        HttpContext httpContext,
        IHostEnvironment environment,
        Exception exception,
        int statusCode,
        string message,
        CancellationToken cancellationToken
    )
    {
        var errors = environment.IsDevelopment()
            ? new Dictionary<string, string[]> { ["exception"] = [exception.ToString()] }
            : null;

        var body = new ApiResponse<object>(false, null, message, errors, httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(body, cancellationToken);
    }
}
