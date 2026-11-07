using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Api.Common;

// The one place a Result becomes an HTTP response.
public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, HttpContext http)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(new ApiResponse<T>(true, result.Value, null, null, http.TraceIdentifier));
        }

        var error = result.Error!;
        var body = new ApiResponse<object>(false, null, error.Message, error.Details, http.TraceIdentifier);

        return Results.Json(body, statusCode: ToStatusCode(error.Type));
    }

    private static int ToStatusCode(ErrorType type)
    {
        if (type == ErrorType.Validation)
        {
            return StatusCodes.Status400BadRequest;
        }

        if (type == ErrorType.Unauthorized)
        {
            return StatusCodes.Status401Unauthorized;
        }

        if (type == ErrorType.Forbidden)
        {
            return StatusCodes.Status403Forbidden;
        }

        if (type == ErrorType.NotFound)
        {
            return StatusCodes.Status404NotFound;
        }

        return StatusCodes.Status409Conflict;
    }
}
