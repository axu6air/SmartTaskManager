namespace SmartTaskManager.Domain.Exceptions;

public abstract class BaseException : Exception
{
    /// <summary>
    /// Optional user-facing message forwarded to the frontend by the exception handlers.
    /// When null, <see cref="Exception.Message"/> is used instead.
    /// </summary>
    public string? UiMessage { get; init; }

    protected BaseException() { }

    protected BaseException(string message, params object?[] args)
        : base(FormatMessage(message, args)) { }

    protected BaseException(Exception exception, string message, params object?[] args)
        : base(FormatMessage(message, args), exception) { }

    protected BaseException(string? uiMessage, string message, params object?[] args)
        : base(FormatMessage(message, args))
    {
        UiMessage = uiMessage;
    }

    protected BaseException(
        Exception exception,
        string? uiMessage,
        string message,
        params object?[] args
    )
        : base(FormatMessage(message, args), exception)
    {
        UiMessage = uiMessage;
    }

    private static string FormatMessage(string message, params object?[] args)
    {
        if (args == null || args.Length == 0)
            return message;

        try
        {
            return string.Format(message, args);
        }
        catch (FormatException)
        {
            return message;
        }
    }
}
