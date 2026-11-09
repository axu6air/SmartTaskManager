using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Users.Queries.SearchUsers;

namespace SmartTaskManager.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapGet(
                "/",
                async (HttpContext http, ISender sender, CancellationToken ct, string? search = null) =>
                    (await sender.Send(new SearchUsersQuery(search), ct)).ToHttpResult(http)
            )
            .WithName("SearchUsers")
            .WithSummary("Finds active users by name or email (at most 20), e.g. to add project members.")
            .Produces<ApiResponse<IReadOnlyCollection<UserSummary>>>()
            .ProducesErrors(StatusCodes.Status400BadRequest, StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden)
            .Authorize(Permissions.Users.View);

        return app;
    }
}
