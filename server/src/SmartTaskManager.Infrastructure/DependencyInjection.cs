using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Domain.Repositories;
using SmartTaskManager.Infrastructure.Persistence;
using SmartTaskManager.Infrastructure.Persistence.Initialization;
using SmartTaskManager.Infrastructure.Repositories;
using SmartTaskManager.Infrastructure.Security;

namespace SmartTaskManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddJwtConfiguration();

        services.AddSingleton<SlowQueryInterceptor>();
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var database = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

            options
                .UseSqlServer(database.ConnectionString, sql => sql.EnableRetryOnFailure(
                    database.MaxRetryCount,
                    TimeSpan.FromSeconds(database.MaxRetryDelaySeconds),
                    errorNumbersToAdd: null))
                .AddInterceptors(
                    serviceProvider.GetRequiredService<SlowQueryInterceptor>(),
                    serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
        });

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();

        services.AddSingleton<ITokenService, TokenService>();
        services.AddScoped<IPermissionService, PermissionService>();

        services.AddScoped<DatabaseCreator>();
        services.AddScoped<DemoDataSeeder>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionModuleRepository, PermissionModuleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectTaskRepository, ProjectTaskRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        return services;
    }
}
