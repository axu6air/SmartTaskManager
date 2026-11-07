using MediatR;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Features.Auth.Commands.Login;

namespace SmartTaskManager.Api.Endpoints;

public static class AuthEndpoints
{
    public sealed record LoginRequest(string Email, string Password);

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group
            .MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Signs in with email and password and returns an access token and a refresh token.")
            .Produces<ApiResponse<LoginResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        HttpContext http,
        ISender sender,
        CancellationToken ct
    )
    {
        var command = new LoginCommand(request.Email, request.Password, http.Connection.RemoteIpAddress?.ToString());

        return (await sender.Send(command, ct)).ToHttpResult(http);
    }
}
