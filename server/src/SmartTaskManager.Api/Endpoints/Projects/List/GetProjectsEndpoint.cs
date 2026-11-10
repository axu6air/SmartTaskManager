using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Projects;
using SmartTaskManager.Application.Features.Projects.Queries.GetProjects;

namespace SmartTaskManager.Api.Endpoints.Projects.List;

public static class GetProjectsEndpoint
{
    public static void MapGetProjects(this IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/",
                async (
                    HttpContext http,
                    ISender sender,
                    CancellationToken ct,
                    int page = 1,
                    int pageSize = 30,
                    string? search = null,
                    string? sortBy = null,
                    string? sortDirection = null
                ) =>
                {
                    var result = await sender.Send(
                        new GetProjectsQuery(page, pageSize, search, sortBy, sortDirection),
                        ct
                    );
                    return result.ToHttpResult(http);
                }
            )
            .WithName("GetProjects")
            .WithSummary(
                "Lists the projects the user may see, one page at a time. search matches name or description; "
                    + "sortBy is name, startDate, endDate or createdAt; sortDirection is asc or desc."
            )
            .Produces<ApiResponse<PagedResponse<ProjectSummary>>>()
            .ProducesErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden
            )
            .Authorize(Permissions.Projects.View);
    }
}
