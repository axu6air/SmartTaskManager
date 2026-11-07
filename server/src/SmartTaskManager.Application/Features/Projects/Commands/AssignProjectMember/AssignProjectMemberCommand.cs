using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Projects.Commands.AssignProjectMember;

// Adds the user to the project, or changes their manager flag when they are already a member.
public sealed record AssignProjectMemberCommand(Guid ProjectId, Guid UserId, bool IsManager)
    : IRequest<Result<ProjectResponse>>;
