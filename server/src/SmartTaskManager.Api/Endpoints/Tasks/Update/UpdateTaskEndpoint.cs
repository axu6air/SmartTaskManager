using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Tasks;
using SmartTaskManager.Application.Features.Tasks.Commands.UpdateTask;

namespace SmartTaskManager.Api.Endpoints.Tasks.Update;

public static class UpdateTaskEndpoint
{
    public static void MapUpdateTask(this IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/{taskId:guid}",
                async (
                    Guid projectId,
                    Guid taskId,
                    UpdateTaskRequest request,
                    HttpContext http,
                    ISender sender,
                    CancellationToken ct
                ) =>
                {
                    var command = new UpdateTaskCommand(
                        projectId,
                        taskId,
                        request.Title,
                        request.Description,
                        request.Priority,
                        request.DueDate,
                        request.IsAiModified
                    );

                    var result = await sender.Send(command, ct);
                    return result.ToHttpResult(http);
                }
            )
            .WithName("UpdateTask")
            .WithSummary("Updates a task's title, description, priority and due date.")
            .Produces<ApiResponse<ProjectTaskResponse>>()
            .ProducesErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound
            )
            .AddEndpointFilter<ValidationFilter<UpdateTaskRequest>>()
            .Authorize(Permissions.Tasks.Update);
    }
}
