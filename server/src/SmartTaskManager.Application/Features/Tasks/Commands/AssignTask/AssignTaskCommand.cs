using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Tasks.Commands.AssignTask;

// A null AssigneeId unassigns the task.
public sealed record AssignTaskCommand(Guid ProjectId, Guid TaskId, Guid? AssigneeId)
    : IRequest<Result<ProjectTaskResponse>>;
