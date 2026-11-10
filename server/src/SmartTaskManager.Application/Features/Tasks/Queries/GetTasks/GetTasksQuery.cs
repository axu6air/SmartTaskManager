using System.Linq.Expressions;
using MediatR;
using SmartTaskManager.Application.Common;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Application.Features.Tasks.Queries.GetTasks;

// Search matches title or description; the other filters are exact. SortBy is one of TaskSorts.Fields.
public sealed record GetTasksQuery(
    Guid ProjectId,
    int Page,
    int PageSize,
    string? Search = null,
    ProjectTaskStatus? Status = null,
    ProjectTaskPriority? Priority = null,
    Guid? AssigneeId = null,
    string? SortBy = null,
    string? SortDirection = null
) : IRequest<Result<PagedResponse<ProjectTaskResponse>>>;

public static class TaskSorts
{
    public static readonly SortFields<ProjectTask> Fields = new(
        "dueDate",
        new Dictionary<string, Expression<Func<ProjectTask, object?>>>
        {
            ["title"] = t => t.Title,
            // Tasks without a due date sort last when ascending, instead of first as NULL would.
            ["dueDate"] = t => t.DueDate ?? DateTimeOffset.MaxValue,
            ["priority"] = t => t.Priority,
            ["status"] = t => t.Status,
            ["createdAt"] = t => t.CreatedAt,
        }
    );
}
