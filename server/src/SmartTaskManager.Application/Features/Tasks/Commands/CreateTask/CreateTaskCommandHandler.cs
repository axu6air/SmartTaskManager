using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Domain.Models.Projects;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Tasks.Commands.CreateTask;

public sealed class CreateTaskCommandHandler(
    ResourceAccess access,
    IProjectTaskRepository tasks,
    ICurrentUser currentUser
) : IRequestHandler<CreateTaskCommand, Result<ProjectTaskResponse>>
{
    public async Task<Result<ProjectTaskResponse>> Handle(CreateTaskCommand request, CancellationToken ct)
    {
        var found = await access.GetProjectAsync(
            currentUser.GetRequiredUserId(),
            request.ProjectId,
            Permissions.Tasks.Create,
            ct
        );

        if (!found.IsSuccess)
        {
            return Result<ProjectTaskResponse>.Failure(found.Error!);
        }

        // The domain checks that the assignee is a member of the project.
        var task = ProjectTask.Create(
            found.Value!,
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            request.AssigneeId
        );

        await tasks.CreateAsync(task, ct);

        return Result<ProjectTaskResponse>.Success(ProjectTaskResponse.From(task));
    }
}
