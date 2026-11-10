using SmartTaskManager.Api.Endpoints.Auth;
using SmartTaskManager.Api.RateLimiting;

namespace SmartTaskManager.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth").RequireRateLimiting(RateLimitPolicies.Auth);

        group.MapLogin();
        group.MapRefreshToken();
        group.MapLogout();
        group.MapGetMe();

        return app;
    }
}
