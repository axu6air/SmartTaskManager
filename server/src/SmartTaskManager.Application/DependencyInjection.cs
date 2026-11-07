using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SmartTaskManager.Application.Common.Behaviors;
using SmartTaskManager.Application.Common.Security;

namespace SmartTaskManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(DomainExceptionBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssembly(assembly);
        services.AddScoped<ResourceAccess>();

        return services;
    }
}
