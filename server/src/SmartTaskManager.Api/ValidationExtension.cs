using FluentValidation;
using SmartTaskManager.Api.Endpoints.Auth;

namespace SmartTaskManager.Api;

public static class ValidationExtension
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();

        return services;
    }
}
