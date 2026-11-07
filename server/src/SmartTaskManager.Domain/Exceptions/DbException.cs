namespace SmartTaskManager.Domain.Exceptions;

public abstract class DbException : BaseException
{
    protected DbException() { }

    protected DbException(string message, params object?[] args)
        : base(message, args) { }

    protected DbException(Exception exception, string message, params object?[] args)
        : base(exception, message, args) { }

    /// <summary>
    /// Creates an exception with a user-facing <paramref name="uiMessage"/>. The technical
    /// <paramref name="message"/> and its format arguments come last:
    /// <c>new DbException("shown to the user", "technical {0}", id)</c>.
    /// </summary>
    protected DbException(string? uiMessage, string message, params object?[] args)
        : base(uiMessage, message, args) { }

    /// <inheritdoc cref="DbException(string, string, object?[])"/>
    protected DbException(
        Exception exception,
        string? uiMessage,
        string message,
        params object?[] args
    )
        : base(exception, uiMessage, message, args) { }
}
