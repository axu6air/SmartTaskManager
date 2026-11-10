using SmartTaskManager.Api.Endpoints.Ai.EnhanceDescription;

namespace SmartTaskManager.Api.Endpoints;

public static class AiEndpoints
{
    public static IEndpointRouteBuilder MapAiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ai").WithTags("AI");

        group.MapEnhanceTaskDescription();

        return app;
    }
}
