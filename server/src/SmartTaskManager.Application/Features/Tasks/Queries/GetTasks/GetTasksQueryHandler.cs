using System.Linq.Expressions;
using MediatR;
using SmartTaskManager.Application.Common;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Domain.Models.Projects;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Tasks.Queries.GetTasks;

public sealed class GetTasksQueryHandler(
    IProjectRepository projects,
    IProjectTaskRepository tasks,
    IPermissionService permissions,
    ICurrentUser currentUser
) : IRequestHandler<GetTasksQuery, Result<PagedResponse<ProjectTaskResponse>>>
{
    public async Task<Result<PagedResponse<ProjectTaskResponse>>> Handle(GetTasksQuery request, CancellationToken ct)
    {
        if (!await projects.ExistsAsync(request.ProjectId, ct))
        {
            return Result<PagedResponse<ProjectTaskResponse>>.Failure(Errors.NotFound("Project"));
        }

        var userId = currentUser.GetRequiredUserId();
        var scopes = await permissions.GetScopesAsync(userId, Permissions.Tasks.View, ct) ?? default;
        var (page, pageSize) = Paging.Normalize(request.Page, request.PageSize);
        var (sortBy, ascending) = TaskSorts.Fields.Resolve(request.SortBy, request.SortDirection);

        // The scope is part of the query, so the user only ever receives tasks they may see.
        var result = await tasks.FindResultSetAsync(
            page,
            pageSize,
            predicate: Filter(ScopeFilters.Tasks(userId, scopes, request.ProjectId), request),
            sortBy: sortBy,
            ascending: ascending,
            ct: ct
        );

        return Result<PagedResponse<ProjectTaskResponse>>.Success(
            new PagedResponse<ProjectTaskResponse>(
                result.Data.Select(ProjectTaskResponse.From).ToList(),
                page,
                pageSize,
                result.TotalCount
            )
        );
    }

    private static Expression<Func<ProjectTask, bool>> Filter(
        Expression<Func<ProjectTask, bool>> predicate,
        GetTasksQuery request
    )
    {
        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            predicate = predicate.And(t =>
                t.Title.Contains(search) || (t.Description != null && t.Description.Contains(search)));
        }

        if (request.Status is { } status)
        {
            predicate = predicate.And(t => t.Status == status);
        }

        if (request.Priority is { } priority)
        {
            predicate = predicate.And(t => t.Priority == priority);
        }

        if (request.AssigneeId is { } assigneeId)
        {
            predicate = predicate.And(t => t.AssigneeId == assigneeId);
        }

        return predicate;
    }
}
