using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Ai.Commands.EnhanceTaskDescription;

namespace SmartTaskManager.Api.Endpoints.Ai.EnhanceDescription;

public sealed record EnhanceTaskDescriptionRequest(string? Title, string Description);

public static class EnhanceTaskDescriptionEndpoint
{
    public static void MapEnhanceTaskDescription(this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/task-description",
                async (
                    EnhanceTaskDescriptionRequest request,
                    HttpContext http,
                    ISender sender,
                    CancellationToken ct
                ) =>
                {
                    var command = new EnhanceTaskDescriptionCommand(
                        request.Title,
                        request.Description
                    );

                    var result = await sender.Send(command, ct);
                    return result.ToHttpResult(http);
                }
            )
            .WithName("EnhanceTaskDescription")
            .WithSummary("Suggests an improved task description. The suggestion is not saved.")
            .Produces<ApiResponse<EnhanceTaskDescriptionResponse>>()
            .ProducesErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status502BadGateway,
                StatusCodes.Status503ServiceUnavailable,
                StatusCodes.Status504GatewayTimeout
            )
            .Authorize(Permissions.Ai.EnhanceDescription);
    }
}
