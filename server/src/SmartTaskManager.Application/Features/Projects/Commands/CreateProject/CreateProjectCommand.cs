using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Projects.Commands.CreateProject;

public sealed record CreateProjectCommand(
    string Name,
    string? Description,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate
) : IRequest<Result<ProjectResponse>>, IProjectFields;
