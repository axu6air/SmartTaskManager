using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Tasks.Commands.DeleteTask;

public sealed record DeleteTaskCommand(Guid ProjectId, Guid TaskId) : IRequest<Result<Unit>>;
