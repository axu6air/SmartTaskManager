using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Tasks;
using SmartTaskManager.Application.Features.Tasks.Queries.GetTasks;
using SmartTaskManager.Domain.Models.Projects;

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
                    int pageSize = 30,
                    string? search = null,
                    ProjectTaskStatus? status = null,
                    ProjectTaskPriority? priority = null,
                    Guid? assigneeId = null,
                    string? sortBy = null,
                    string? sortDirection = null
                ) =>
                {
                    var query = new GetTasksQuery(
                        projectId,
                        page,
                        pageSize,
                        search,
                        status,
                        priority,
                        assigneeId,
                        sortBy,
                        sortDirection
                    );
                    var result = await sender.Send(query, ct);
                    return result.ToHttpResult(http);
                }
            )
            .WithName("GetTasks")
            .WithSummary(
                "Lists the tasks of a project the user may see, one page at a time. search matches title or "
                    + "description; status, priority and assigneeId filter exactly; sortBy is title, dueDate "
                    + "(no due date last), priority, status or createdAt; sortDirection is asc or desc."
            )
            .Produces<ApiResponse<PagedResponse<ProjectTaskResponse>>>()
            .ProducesErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound
            )
            .Authorize(Permissions.Tasks.View);
    }
}
