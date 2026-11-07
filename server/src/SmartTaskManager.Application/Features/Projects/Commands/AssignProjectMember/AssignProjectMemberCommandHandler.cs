using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Projects.Commands.AssignProjectMember;

public sealed class AssignProjectMemberCommandHandler(
    ResourceAccess access,
    IProjectRepository projects,
    IUserRepository users,
    ICurrentUser currentUser
) : IRequestHandler<AssignProjectMemberCommand, Result<ProjectResponse>>
{
    public async Task<Result<ProjectResponse>> Handle(AssignProjectMemberCommand request, CancellationToken ct)
    {
        var found = await access.GetProjectAsync(
            currentUser.GetRequiredUserId(),
            request.ProjectId,
            Permissions.Projects.ManageMembers,
            ct
        );

        if (!found.IsSuccess)
        {
            return Result<ProjectResponse>.Failure(found.Error!);
        }

        if (!await users.ExistsAsync(request.UserId, ct))
        {
            return Result<ProjectResponse>.Failure(Errors.NotFound("User"));
        }

        var project = found.Value!;

        if (project.IsMember(request.UserId))
        {
            project.SetManager(request.UserId, request.IsManager);
        }
        else
        {
            project.AddMember(request.UserId, request.IsManager);
        }

        await projects.SaveChangesAsync(ct);

        return Result<ProjectResponse>.Success(ProjectResponse.From(project));
    }
}
