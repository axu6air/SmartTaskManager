using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace SmartTaskManager.Api.Cors;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services
            .AddOptions<CorsSettings>()
            .BindConfiguration(CorsSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddCors();

        // Configured through options so the validated CorsSettings is the single source of the origins.
        // No credentials: the client sends the access token in the Authorization header, not a cookie.
        services
            .AddOptions<CorsOptions>()
            .Configure<IOptions<CorsSettings>>(
                (cors, settings) =>
                    cors.AddDefaultPolicy(policy =>
                        policy
                            .WithOrigins(settings.Value.AllowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .WithExposedHeaders("Location", "Retry-After")
                    )
            );

        return services;
    }
}
