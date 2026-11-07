using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Tasks.Commands.DeleteTask;

public sealed class DeleteTaskCommandHandler(
    ResourceAccess access,
    IProjectTaskRepository tasks,
    ICurrentUser currentUser,
    TimeProvider timeProvider
) : IRequestHandler<DeleteTaskCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteTaskCommand request, CancellationToken ct)
    {
        var userId = currentUser.GetRequiredUserId();
        var found = await access.GetTaskAsync(
            userId,
            request.ProjectId,
            request.TaskId,
            Permissions.Tasks.Delete,
            ct
        );

        if (!found.IsSuccess)
        {
            return Result<Unit>.Failure(found.Error!);
        }

        found.Value!.MarkDeleted(userId, timeProvider.GetUtcNow());
        await tasks.SaveChangesAsync(ct);

        return Result<Unit>.Success(Unit.Value);
    }
}
