namespace SmartTaskManager.Infrastructure.Errors;

public enum RepositoryErrorType
{
    Unknown,
    CancellationRequested,
    CommandTimeout,
    ConnectionTimeout,
    RetryPolicyTimeout,
    Deadlock,
    ConcurrencyConflict,
    UpdateFailed,
    SqlError,
    Timeout,
    RetryExhausted,
}
