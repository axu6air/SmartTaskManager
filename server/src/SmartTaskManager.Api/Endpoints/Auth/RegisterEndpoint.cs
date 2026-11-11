using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Auth.Commands.Register;

namespace SmartTaskManager.Api.Endpoints.Auth;

public sealed record RegisterUserRequest(string FullName, string Email, string Password, Guid? RoleId);

public static class RegisterEndpoint
{
    public static void MapRegister(this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/register",
                async (RegisterUserRequest request, HttpContext http, ISender sender, CancellationToken ct) =>
                {
                    var command = new RegisterUserCommand(
                        request.FullName,
                        request.Email,
                        request.Password,
                        request.RoleId
                    );

                    var result = await sender.Send(command, ct);
                    return result.ToHttpResult(http, user => $"/api/users/{user.Id}");
                }
            )
            .WithName("RegisterUser")
            .WithSummary(
                "Creates a user account (Manage Users permission). Without roleId the user gets the default role."
            )
            .Produces<ApiResponse<RegisterUserResponse>>(StatusCodes.Status201Created)
            .ProducesErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound,
                StatusCodes.Status409Conflict
            )
            .Authorize(Permissions.Users.Manage);
    }
}
