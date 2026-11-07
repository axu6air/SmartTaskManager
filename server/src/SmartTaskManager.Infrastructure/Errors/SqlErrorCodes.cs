namespace SmartTaskManager.Infrastructure.Errors;

internal static class SqlErrorCodes
{
    internal const int Timeout = -2;
    internal const int ConnectionTimeout = -1;
    internal const int ConnectionBroken = 1231;
    internal const int Deadlock = 1205;
    internal const int ConstraintViolation = 547;
    internal const int DuplicateKey = 2627;
    internal const int CannotOpenDatabase = 4060;
}
