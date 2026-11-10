namespace SmartTaskManager.Application.Common.Results;

public enum ErrorType
{
    Validation,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
    ServiceUnavailable,
}

public sealed record Error(
    ErrorType Type,
    string Message,
    IReadOnlyDictionary<string, string[]>? Details = null
);

// Errors shared by several features.
public static class Errors
{
    public static readonly Error Forbidden = new(ErrorType.Forbidden, "You do not have access to this resource.");

    public static Error NotFound(string resource) => new(ErrorType.NotFound, $"{resource} was not found.");
}

// Lets pipeline behaviors build a failed result for any handler response without reflection.
public interface IFailureResult<TSelf>
    where TSelf : IFailureResult<TSelf>
{
    static abstract TSelf Failure(Error error);
}

public sealed class Result<T> : IFailureResult<Result<T>>
{
    private Result(T? value, Error? error)
    {
        Value = value;
        Error = error;
    }

    public bool IsSuccess => Error is null;

    public T? Value { get; }

    public Error? Error { get; }

    public static Result<T> Success(T value) => new(value, null);

    public static Result<T> Failure(Error error) => new(default, error);

    public Result<TOut> Map<TOut>(Func<T, TOut> map) =>
        IsSuccess ? Result<TOut>.Success(map(Value!)) : Result<TOut>.Failure(Error!);
}
