using SmartTaskManager.Application.Common.Exceptions;

namespace SmartTaskManager.Application.Common.Abstractions;

public interface ITaskDescriptionEnhancer
{
    // Returns the improved task. Throws AiServiceException when the AI cannot produce one.
    Task<EnhancedTask> EnhanceAsync(string? title, string description, CancellationToken ct);
}

// Both fields fit dbo.ProjectTasks: Title at most 200 characters, Description at most 4000.
public sealed record EnhancedTask(string Title, string Description);
