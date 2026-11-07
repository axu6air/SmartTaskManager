using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Api.Common;

// The one place a Result becomes an HTTP response.
public static class ResultExtensions
{
    // With createdAt the success response is 201 with a Location header, otherwise 200.
    public static IResult ToHttpResult<T>(this Result<T> result, HttpContext http, Func<T, string>? createdAt = null)
    {
        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Error!, http);
        }

        var body = new ApiResponse<T>(true, result.Value, null, null, http.TraceIdentifier);

        return createdAt is null ? Results.Ok(body) : Results.Created(createdAt(result.Value!), body);
    }

    public static IResult ToNoContentHttpResult<T>(this Result<T> result, HttpContext http) =>
        result.IsSuccess ? Results.NoContent() : ToErrorResult(result.Error!, http);

    private static IResult ToErrorResult(Error error, HttpContext http)
    {
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
