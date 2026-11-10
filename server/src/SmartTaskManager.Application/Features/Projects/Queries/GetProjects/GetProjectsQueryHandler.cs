using MediatR;
using SmartTaskManager.Application.Common;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Projects.Queries.GetProjects;

public sealed class GetProjectsQueryHandler(
    IProjectRepository projects,
    IPermissionService permissions,
    ICurrentUser currentUser
) : IRequestHandler<GetProjectsQuery, Result<PagedResponse<ProjectSummary>>>
{
    public async Task<Result<PagedResponse<ProjectSummary>>> Handle(
        GetProjectsQuery request,
        CancellationToken ct
    )
    {
        var userId = currentUser.GetRequiredUserId();
        var scopes =
            await permissions.GetScopesAsync(userId, Permissions.Projects.View, ct) ?? default;
        var (page, pageSize) = Paging.Normalize(request.Page, request.PageSize);
        var (sortBy, ascending) = ProjectSorts.Fields.Resolve(request.SortBy, request.SortDirection);

        // The scope is part of the query, so the user only ever receives projects they may see.
        var predicate = ScopeFilters.Projects(userId, scopes);
        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            predicate = predicate.And(p =>
                p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));
        }

        var result = await projects.FindResultSetAsync(
            page,
            pageSize,
            predicate: predicate,
            sortBy: sortBy,
            ascending: ascending,
            ct: ct
        );

        return Result<PagedResponse<ProjectSummary>>.Success(
            new PagedResponse<ProjectSummary>(
                [.. result.Data.Select(ProjectSummary.From)],
                page,
                pageSize,
                result.TotalCount
            )
        );
    }
}
