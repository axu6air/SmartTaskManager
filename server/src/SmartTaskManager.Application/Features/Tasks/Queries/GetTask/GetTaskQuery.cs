using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Tasks.Queries.GetTask;

public sealed record GetTaskQuery(Guid ProjectId, Guid TaskId) : IRequest<Result<ProjectTaskResponse>>;
