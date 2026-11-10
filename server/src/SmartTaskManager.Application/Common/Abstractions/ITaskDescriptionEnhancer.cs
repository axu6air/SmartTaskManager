using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Common.Abstractions;

public interface ITaskDescriptionEnhancer
{
    // Returns the improved task, or a ServiceUnavailable failure when the AI cannot produce one.
    Task<Result<EnhancedTask>> EnhanceAsync(string? title, string description, CancellationToken ct);
}

// Both fields fit dbo.ProjectTasks: Title at most 200 characters, Description at most 4000.
public sealed record EnhancedTask(string Title, string Description);
