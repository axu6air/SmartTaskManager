using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Tasks.Commands.UpdateTask;

public sealed class UpdateTaskCommandHandler(
    ResourceAccess access,
    IProjectTaskRepository tasks,
    ICurrentUser currentUser
) : IRequestHandler<UpdateTaskCommand, Result<ProjectTaskResponse>>
{
    public async Task<Result<ProjectTaskResponse>> Handle(UpdateTaskCommand request, CancellationToken ct)
    {
        var found = await access.GetTaskAsync(
            currentUser.GetRequiredUserId(),
            request.ProjectId,
            request.TaskId,
            Permissions.Tasks.Update,
            ct
        );

        if (!found.IsSuccess)
        {
            return Result<ProjectTaskResponse>.Failure(found.Error!);
        }

        var task = found.Value!;
        task.Update(
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            request.IsAiModified
        );
        await tasks.SaveChangesAsync(ct);

        return Result<ProjectTaskResponse>.Success(ProjectTaskResponse.From(task));
    }
}
