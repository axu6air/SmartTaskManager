using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Tasks;
using SmartTaskManager.Application.Features.Tasks.Commands.ChangeTaskStatus;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Api.Endpoints.Tasks.Status;

// Status is the number: 1 ToDo, 2 InProgress, 3 Completed, 4 Cancelled.
public sealed record ChangeTaskStatusRequest(ProjectTaskStatus Status);

public static class ChangeTaskStatusEndpoint
{
    public static void MapChangeTaskStatus(this IEndpointRouteBuilder app)
    {
        app.MapPatch(
                "/{taskId:guid}/status",
                async (
                    Guid projectId,
                    Guid taskId,
                    ChangeTaskStatusRequest request,
                    HttpContext http,
                    ISender sender,
                    CancellationToken ct
                ) =>
                {
                    var command = new ChangeTaskStatusCommand(projectId, taskId, request.Status);

                    var result = await sender.Send(command, ct);
                    return result.ToHttpResult(http);
                }
            )
            .WithName("ChangeTaskStatus")
            .WithSummary("Moves a task to another status.")
            .Produces<ApiResponse<ProjectTaskResponse>>()
            .ProducesErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound
            )
            .Authorize(Permissions.Tasks.UpdateStatus);
    }
}
