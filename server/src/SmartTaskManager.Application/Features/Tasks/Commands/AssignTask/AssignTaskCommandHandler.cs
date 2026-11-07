using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Tasks.Commands.AssignTask;

public sealed class AssignTaskCommandHandler(
    ResourceAccess access,
    IProjectTaskRepository tasks,
    ICurrentUser currentUser
) : IRequestHandler<AssignTaskCommand, Result<ProjectTaskResponse>>
{
    public async Task<Result<ProjectTaskResponse>> Handle(AssignTaskCommand request, CancellationToken ct)
    {
        var found = await access.GetTaskAsync(
            currentUser.GetRequiredUserId(),
            request.ProjectId,
            request.TaskId,
            Permissions.Tasks.Assign,
            ct
        );

        if (!found.IsSuccess)
        {
            return Result<ProjectTaskResponse>.Failure(found.Error!);
        }

        // The domain checks that the assignee is a member of the task's project.
        var task = found.Value!;
        task.Assign(task.Project, request.AssigneeId);
        await tasks.SaveChangesAsync(ct);

        return Result<ProjectTaskResponse>.Success(ProjectTaskResponse.From(task));
    }
}
