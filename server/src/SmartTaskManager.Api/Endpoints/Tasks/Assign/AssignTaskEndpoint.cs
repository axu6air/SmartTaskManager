using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Tasks;
using SmartTaskManager.Application.Features.Tasks.Commands.AssignTask;

namespace SmartTaskManager.Api.Endpoints.Tasks.Assign;

public static class AssignTaskEndpoint
{
    public static void MapAssignTask(this IEndpointRouteBuilder app)
    {
        app.MapPatch(
                "/{taskId:guid}/assign",
                async (
                    Guid projectId,
                    Guid taskId,
                    AssignTaskRequest request,
                    HttpContext http,
                    ISender sender,
                    CancellationToken ct
                ) =>
                {
                    var command = new AssignTaskCommand(projectId, taskId, request.AssigneeId);

                    var result = await sender.Send(command, ct);
                    return result.ToHttpResult(http);
                }
            )
            .WithName("AssignTask")
            .WithSummary(
                "Assigns a task to a member of its project, or unassigns it when assigneeId is null."
            )
            .Produces<ApiResponse<ProjectTaskResponse>>()
            .ProducesErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound
            )
            .Authorize(Permissions.Tasks.Assign);
    }
}
