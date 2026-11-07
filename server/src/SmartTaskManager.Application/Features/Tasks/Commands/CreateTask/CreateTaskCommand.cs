using MediatR;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Application.Features.Tasks.Commands.CreateTask;

public sealed record CreateTaskCommand(
    Guid ProjectId,
    string Title,
    string? Description,
    ProjectTaskPriority Priority,
    DateTimeOffset? DueDate,
    Guid? AssigneeId
) : IRequest<Result<ProjectTaskResponse>>, ITaskFields;
