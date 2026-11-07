namespace SmartTaskManager.Api.Common;

public sealed record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Message,
    IReadOnlyDictionary<string, string[]>? Errors,
    string TraceId
);
