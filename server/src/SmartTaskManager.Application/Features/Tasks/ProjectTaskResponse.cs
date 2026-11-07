using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Application.Features.Tasks;

public sealed record ProjectTaskResponse(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    ProjectTaskStatus Status,
    ProjectTaskPriority Priority,
    DateTimeOffset? DueDate,
    Guid? AssigneeId,
    DateTimeOffset? CompletedAt
)
{
    public static ProjectTaskResponse From(ProjectTask task) =>
        new(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.AssigneeId,
            task.CompletedAt
        );
}
