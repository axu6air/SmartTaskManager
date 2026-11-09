using MediatR;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Application.Features.Tasks.Commands.ChangeTaskStatus;

public sealed record ChangeTaskStatusCommand(Guid ProjectId, Guid TaskId, ProjectTaskStatus Status)
    : IRequest<Result<ProjectTaskResponse>>;
