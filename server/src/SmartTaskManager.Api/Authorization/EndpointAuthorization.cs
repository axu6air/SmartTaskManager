using SmartTaskManager.Api.Authentication;
using SmartTaskManager.Application.Common.Abstractions;

namespace SmartTaskManager.Api.Authorization;

public sealed record AuthenticationRequirement;

public sealed record PermissionRequirement(string Code);

// Runs after UseAuthentication and endpoint routing. Endpoints without a requirement pass through untouched.
public sealed class EndpointAuthorizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IPermissionService permissions)
    {
        // The metadata nearest the endpoint wins, so an endpoint can override its group.
        var permission = context.GetEndpoint()?.Metadata.GetMetadata<PermissionRequirement>();
        var needsAuthentication =
            permission is not null
            || context.GetEndpoint()?.Metadata.GetMetadata<AuthenticationRequirement>() is not null;

        if (needsAuthentication)
        {
            if (context.User.GetUserId() is not { } userId)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            if (
                permission is not null
                && await permissions.GetScopesAsync(userId, permission.Code, context.RequestAborted)
                    is null
            )
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }

        await next(context);
    }
}
