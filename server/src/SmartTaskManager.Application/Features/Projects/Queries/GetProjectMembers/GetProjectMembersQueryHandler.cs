using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Projects.Queries.GetProjectMembers;

// Names for the project's members, so clients can show assignees without a user-management permission.
public sealed class GetProjectMembersQueryHandler(
    ResourceAccess access,
    IUserRepository users,
    ICurrentUser currentUser
) : IRequestHandler<GetProjectMembersQuery, Result<IReadOnlyCollection<ProjectMemberDetails>>>
{
    public async Task<Result<IReadOnlyCollection<ProjectMemberDetails>>> Handle(
        GetProjectMembersQuery request,
        CancellationToken ct
    )
    {
        var found = await access.GetProjectAsync(
            currentUser.GetRequiredUserId(),
            request.ProjectId,
            Permissions.Projects.View,
            ct
        );

        if (!found.IsSuccess)
        {
            return Result<IReadOnlyCollection<ProjectMemberDetails>>.Failure(found.Error!);
        }

        var members = found.Value!.Members.ToDictionary(m => m.UserId, m => m.IsManager);
        var memberIds = members.Keys.ToList();
        var people = await users.FindAllAsync(u => memberIds.Contains(u.Id), sortBy: u => u.FullName, ct: ct);

        return Result<IReadOnlyCollection<ProjectMemberDetails>>.Success(
            people.Select(u => new ProjectMemberDetails(u.Id, u.FullName, u.Email, members[u.Id])).ToList()
        );
    }
}
