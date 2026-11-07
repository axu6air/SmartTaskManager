using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Tasks.Commands.DeleteTask;

namespace SmartTaskManager.Api.Endpoints.Tasks.Delete;

public static class DeleteTaskEndpoint
{
    public static void MapDeleteTask(this IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/{taskId:guid}",
                async (
                    Guid projectId,
                    Guid taskId,
                    HttpContext http,
                    ISender sender,
                    CancellationToken ct
                ) =>
                {
                    var result = await sender.Send(new DeleteTaskCommand(projectId, taskId), ct);
                    return result.ToNoContentHttpResult(http);
                }
            )
            .WithName("DeleteTask")
            .WithSummary("Deletes a task (soft delete).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesErrors(
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound
            )
            .Authorize(Permissions.Tasks.Delete);
    }
}
