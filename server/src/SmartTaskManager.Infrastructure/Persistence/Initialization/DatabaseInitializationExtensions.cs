using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SmartTaskManager.Infrastructure.Persistence.Initialization;

public static class DatabaseInitializationExtensions
{
    /// <summary>
    /// Creates the database, its tables and the reference data when they are missing, then adds the demo users,
    /// projects and tasks. Call it in Development only. A failure is logged and does not stop the app.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DatabaseInitializationExtensions));

        try
        {
            await scope.ServiceProvider.GetRequiredService<DatabaseCreator>().EnsureCreatedAsync(ct);
            await scope.ServiceProvider.GetRequiredService<DemoDataSeeder>().SeedAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Database initialization failed. The app keeps running without it.");
        }
    }
}
