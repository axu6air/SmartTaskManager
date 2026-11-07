using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Tasks;
using SmartTaskManager.Application.Features.Tasks.Queries.GetTask;

namespace SmartTaskManager.Api.Endpoints.Tasks.List;

public static class GetTaskEndpoint
{
    public static void MapGetTask(this IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/{taskId:guid}",
                async (
                    Guid projectId,
                    Guid taskId,
                    HttpContext http,
                    ISender sender,
                    CancellationToken ct
                ) =>
                {
                    var result = await sender.Send(new GetTaskQuery(projectId, taskId), ct);
                    return result.ToHttpResult(http);
                }
            )
            .WithName("GetTask")
            .WithSummary("Gets a task.")
            .Produces<ApiResponse<ProjectTaskResponse>>()
            .ProducesErrors(
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound
            )
            .Authorize(Permissions.Tasks.View);
    }
}
