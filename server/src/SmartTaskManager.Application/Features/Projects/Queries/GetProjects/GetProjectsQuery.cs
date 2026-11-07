using MediatR;
using SmartTaskManager.Application.Common;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Projects.Queries.GetProjects;

public sealed record GetProjectsQuery(int Page, int PageSize)
    : IRequest<Result<PagedResponse<ProjectSummary>>>;
