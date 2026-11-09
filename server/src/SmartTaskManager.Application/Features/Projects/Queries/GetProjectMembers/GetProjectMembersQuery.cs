using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Projects.Queries.GetProjectMembers;

public sealed record GetProjectMembersQuery(Guid ProjectId)
    : IRequest<Result<IReadOnlyCollection<ProjectMemberDetails>>>;

public sealed record ProjectMemberDetails(Guid UserId, string FullName, string Email, bool IsManager);
