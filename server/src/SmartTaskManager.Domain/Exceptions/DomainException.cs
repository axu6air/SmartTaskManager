namespace SmartTaskManager.Domain.Exceptions;

public sealed class DomainException : BaseException
{
    // Business rule messages are written for users, so they are safe to show as they are.
    public DomainException(string message)
        : base(message)
    {
        UiMessage = message;
    }
}
