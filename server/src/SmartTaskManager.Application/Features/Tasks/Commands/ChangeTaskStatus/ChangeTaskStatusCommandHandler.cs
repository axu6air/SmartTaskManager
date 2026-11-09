using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Tasks.Commands.ChangeTaskStatus;

public sealed class ChangeTaskStatusCommandHandler(
    ResourceAccess access,
    IProjectTaskRepository tasks,
    ICurrentUser currentUser,
    TimeProvider timeProvider
) : IRequestHandler<ChangeTaskStatusCommand, Result<ProjectTaskResponse>>
{
    public async Task<Result<ProjectTaskResponse>> Handle(ChangeTaskStatusCommand request, CancellationToken ct)
    {
        var found = await access.GetTaskAsync(
            currentUser.GetRequiredUserId(),
            request.ProjectId,
            request.TaskId,
            Permissions.Tasks.UpdateStatus,
            ct
        );

        if (!found.IsSuccess)
        {
            return Result<ProjectTaskResponse>.Failure(found.Error!);
        }

        var task = found.Value!;
        task.ChangeStatus(request.Status, timeProvider.GetUtcNow());
        await tasks.SaveChangesAsync(ct);

        return Result<ProjectTaskResponse>.Success(ProjectTaskResponse.From(task));
    }
}
