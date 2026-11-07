namespace SmartTaskManager.Application.Common.Results;

public enum ErrorType
{
    Validation,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
}

public sealed record Error(
    ErrorType Type,
    string Message,
    IReadOnlyDictionary<string, string[]>? Details = null
);

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
}
