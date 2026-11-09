using FluentValidation;
using SmartTaskManager.Api.Endpoints.Auth;
using SmartTaskManager.Api.Endpoints.Projects.Create;

namespace SmartTaskManager.Api;

public static class ValidationExtension
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<CreateProjectRequest>, CreateProjectRequestValidator>();

        return services;
    }
}
