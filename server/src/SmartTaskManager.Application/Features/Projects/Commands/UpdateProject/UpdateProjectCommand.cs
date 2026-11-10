using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Projects.Commands.UpdateProject;

public sealed record UpdateProjectCommand(
    Guid ProjectId,
    string Name,
    string? Description,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    bool IsAiModified
) : IRequest<Result<ProjectResponse>>, IProjectFields;
