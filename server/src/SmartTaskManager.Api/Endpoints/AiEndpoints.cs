using SmartTaskManager.Api.Endpoints.Ai.EnhanceDescription;
using SmartTaskManager.Api.RateLimiting;

namespace SmartTaskManager.Api.Endpoints;

public static class AiEndpoints
{
    public static IEndpointRouteBuilder MapAiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ai").WithTags("AI").RequireRateLimiting(RateLimitPolicies.Ai);

        group.MapEnhanceTaskDescription();

        return app;
    }
}
