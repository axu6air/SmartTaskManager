using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Features.Auth.Commands.Login;
using SmartTaskManager.Application.Features.Auth.Commands.Logout;
using SmartTaskManager.Application.Features.Auth.Commands.Refresh;
using SmartTaskManager.Application.Features.Auth.Queries.GetMe;

namespace SmartTaskManager.Api.Endpoints.Auth;

public sealed record RefreshTokenRequest(string RefreshToken);

public static class TokenEndpoints
{
    public static void MapRefreshToken(this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/refresh",
                async (RefreshTokenRequest request, HttpContext http, ISender sender, CancellationToken ct) =>
                {
                    var command = new RefreshTokenCommand(
                        request.RefreshToken,
                        http.Connection.RemoteIpAddress?.ToString()
                    );

                    var result = await sender.Send(command, ct);
                    return result.ToHttpResult(http);
                }
            )
            .WithName("RefreshToken")
            .WithSummary("Exchanges a refresh token for a new access token and a rotated refresh token.")
            .Produces<ApiResponse<LoginResponse>>()
            .ProducesErrors(StatusCodes.Status400BadRequest, StatusCodes.Status401Unauthorized);
    }

    public static void MapLogout(this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/logout",
                async (RefreshTokenRequest request, HttpContext http, ISender sender, CancellationToken ct) =>
                {
                    var command = new LogoutCommand(request.RefreshToken, http.Connection.RemoteIpAddress?.ToString());

                    var result = await sender.Send(command, ct);
                    return result.ToNoContentHttpResult(http);
                }
            )
            .WithName("Logout")
            .WithSummary("Revokes the refresh token.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesErrors(StatusCodes.Status400BadRequest);
    }

    public static void MapGetMe(this IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/me",
                async (HttpContext http, ISender sender, CancellationToken ct) =>
                {
                    var result = await sender.Send(new GetMeQuery(), ct);
                    return result.ToHttpResult(http);
                }
            )
            .WithName("GetMe")
            .WithSummary("Gets the signed-in user's profile, roles and permission scopes.")
            .Produces<ApiResponse<MeResponse>>()
            .ProducesErrors(StatusCodes.Status401Unauthorized)
            .Authenticate();
    }
}
