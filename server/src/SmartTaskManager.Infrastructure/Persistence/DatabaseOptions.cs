using System.ComponentModel.DataAnnotations;

namespace SmartTaskManager.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public required string ConnectionString { get; init; } = string.Empty;

    [Range(0, 20)]
    public int MaxRetryCount { get; init; } = 5;

    [Range(1, 300)]
    public int MaxRetryDelaySeconds { get; init; } = 30;

    // Milliseconds. Commands that take longer are logged as warnings.
    [Range(1, int.MaxValue)]
    public int SlowQueryThreshold { get; init; } = 500;
}
