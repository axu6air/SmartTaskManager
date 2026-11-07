using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Projects.Commands.DeleteProject;

public sealed class DeleteProjectCommandHandler(
    ResourceAccess access,
    IProjectRepository projects,
    ICurrentUser currentUser,
    TimeProvider timeProvider
) : IRequestHandler<DeleteProjectCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteProjectCommand request, CancellationToken ct)
    {
        var userId = currentUser.GetRequiredUserId();
        var found = await access.GetProjectAsync(userId, request.ProjectId, Permissions.Projects.Delete, ct);

        if (!found.IsSuccess)
        {
            return Result<Unit>.Failure(found.Error!);
        }

        // Soft delete: the project's tasks disappear with it because every task is reached through its project.
        found.Value!.MarkDeleted(userId, timeProvider.GetUtcNow());
        await projects.SaveChangesAsync(ct);

        return Result<Unit>.Success(Unit.Value);
    }
}
