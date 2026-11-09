using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Projects.Commands.RemoveProjectMember;

// Removing a member unassigns their tasks in the project, since an assignee must be a member.
// The domain refuses to remove the last manager (a 400 through DomainExceptionBehavior).
public sealed class RemoveProjectMemberCommandHandler(
    ResourceAccess access,
    IProjectRepository projects,
    IProjectTaskRepository tasks,
    ICurrentUser currentUser
) : IRequestHandler<RemoveProjectMemberCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RemoveProjectMemberCommand request, CancellationToken ct)
    {
        var found = await access.GetProjectAsync(
            currentUser.GetRequiredUserId(),
            request.ProjectId,
            Permissions.Projects.ManageMembers,
            ct
        );

        if (!found.IsSuccess)
        {
            return Result<bool>.Failure(found.Error!);
        }

        var project = found.Value!;
        if (!project.IsMember(request.UserId))
        {
            return Result<bool>.Failure(Errors.NotFound("Member"));
        }

        project.RemoveMember(request.UserId);

        foreach (var task in await tasks.FindAssignedIncludingDeletedAsync(project.Id, request.UserId, ct))
        {
            task.Assign(project, null);
        }

        // One context, one save: the unassignments and the membership removal commit together.
        await projects.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
