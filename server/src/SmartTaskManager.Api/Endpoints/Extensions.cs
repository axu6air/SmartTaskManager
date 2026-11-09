namespace SmartTaskManager.Api.Endpoints;

public static class Extensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        return app.MapAuthEndpoints()
            .MapProjectEndpoints()
            .MapTaskEndpoints()
            .MapRoleEndpoints()
            .MapUserEndpoints();
    }
}
