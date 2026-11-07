using SmartTaskManager.Domain.Exceptions;
using SmartTaskManager.Infrastructure.Errors;

namespace SmartTaskManager.Infrastructure.Exceptions;

public sealed class RepositoryException : DbException
{
    public RepositoryException() { }

    public RepositoryException(string message, params object?[] args)
        : base(message, args) { }

    public RepositoryException(Exception exception, string message, params object?[] args)
        : base(exception, message, args) { }

    /// <summary>
    /// Creates an exception with a user-facing <paramref name="uiMessage"/>. The technical
    /// <paramref name="message"/> and its format arguments come last:
    /// <c>new RepositoryException("shown to the user", "technical {0}", id)</c>.
    /// </summary>
    public RepositoryException(string? uiMessage, string message, params object?[] args)
        : base(uiMessage, message, args) { }

    /// <inheritdoc cref="RepositoryException(string, string, object?[])"/>
    public RepositoryException(
        Exception exception,
        string? uiMessage,
        string message,
        params object?[] args
    )
        : base(exception, uiMessage, message, args) { }

    public RepositoryErrorType ErrorType { get; set; }
    public bool IsRetriable { get; set; }
}
