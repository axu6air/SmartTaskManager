using System.Linq.Expressions;
using MediatR;
using SmartTaskManager.Application.Common;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Application.Features.Projects.Queries.GetProjects;

// Search matches name or description. SortBy is one of ProjectSorts.Fields; SortDirection is asc or desc.
public sealed record GetProjectsQuery(
    int Page,
    int PageSize,
    string? Search = null,
    string? SortBy = null,
    string? SortDirection = null
) : IRequest<Result<PagedResponse<ProjectSummary>>>;

public static class ProjectSorts
{
    public static readonly SortFields<Project> Fields = new(
        "name",
        new Dictionary<string, Expression<Func<Project, object?>>>
        {
            ["name"] = p => p.Name,
            ["startDate"] = p => p.StartDate,
            ["endDate"] = p => p.EndDate,
            ["createdAt"] = p => p.CreatedAt,
        }
    );
}
