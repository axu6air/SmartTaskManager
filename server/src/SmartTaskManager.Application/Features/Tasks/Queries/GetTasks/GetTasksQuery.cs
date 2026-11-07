using MediatR;
using SmartTaskManager.Application.Common;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Tasks.Queries.GetTasks;

public sealed record GetTasksQuery(Guid ProjectId, int Page, int PageSize)
    : IRequest<Result<PagedResponse<ProjectTaskResponse>>>;
