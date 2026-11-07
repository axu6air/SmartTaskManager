using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Tasks;
using SmartTaskManager.Application.Features.Tasks.Queries.GetTasks;

namespace SmartTaskManager.Api.Endpoints.Tasks.List;

public static class GetTasksEndpoint
{
    public static void MapGetTasks(this IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/",
                async (
                    Guid projectId,
                    HttpContext http,
                    ISender sender,
                    CancellationToken ct,
                    int page = 1,
                    int pageSize = 30
                ) =>
                {
                    var result = await sender.Send(new GetTasksQuery(projectId, page, pageSize), ct);
                    return result.ToHttpResult(http);
                }
            )
            .WithName("GetTasks")
            .WithSummary("Lists the tasks of a project the user may see, one page at a time.")
            .Produces<ApiResponse<PagedResponse<ProjectTaskResponse>>>()
            .ProducesErrors(
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound
            )
            .Authorize(Permissions.Tasks.View);
    }
}
