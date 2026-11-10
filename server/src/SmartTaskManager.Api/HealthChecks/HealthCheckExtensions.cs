using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace SmartTaskManager.Api.HealthChecks;

// The checks themselves (e.g. the database) are registered by AddInfrastructure.
// Both endpoints are anonymous and skip rate limiting, so probes are never throttled.
public static class HealthCheckExtensions
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/health").WithTags("Health").DisableRateLimiting();

        // Liveness: the process is up and serving requests. Runs no checks, so a slow database
        // does not get the app restarted.
        group.MapHealthChecks("/live", new HealthCheckOptions { Predicate = _ => false });

        // Readiness: every registered check, including the database, must pass.
        group.MapHealthChecks("/ready");

        return app;
    }
}
