using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Projects.Commands.UpdateProject;

public sealed record UpdateProjectCommand(
    Guid ProjectId,
    string Name,
    string? Description,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate
) : IRequest<Result<ProjectResponse>>, IProjectFields;
