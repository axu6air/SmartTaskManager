using MediatR;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Application.Features.Tasks.Commands.UpdateTask;

public sealed record UpdateTaskCommand(
    Guid ProjectId,
    Guid TaskId,
    string Title,
    string? Description,
    ProjectTaskPriority Priority,
    DateTimeOffset? DueDate,
    bool IsAiModified
) : IRequest<Result<ProjectTaskResponse>>, ITaskFields;
