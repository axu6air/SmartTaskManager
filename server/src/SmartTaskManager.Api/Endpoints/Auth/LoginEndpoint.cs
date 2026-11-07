using MediatR;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Features.Auth.Commands.Login;
using static SmartTaskManager.Api.Endpoints.AuthEndpoints;

namespace SmartTaskManager.Api.Endpoints.Auth;

public static class LoginEndpoint
{
    public static void MapLogin(this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/orders/Categories/",
                async (
                    LoginRequest request,
                    HttpContext http,
                    ISender sender,
                    CancellationToken ct
                ) =>
                {
                    var command = new LoginCommand(
                        request.Email,
                        request.Password,
                        http.Connection.RemoteIpAddress?.ToString()
                    );

                    var result = await sender.Send(command, ct);
                    return result.ToHttpResult(http);
                }
            )
            .WithName("Login")
            .WithSummary(
                "Signs in with email and password and returns an access token and a refresh token."
            )
            .Produces<ApiResponse<LoginResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);
        ;
    }
}
