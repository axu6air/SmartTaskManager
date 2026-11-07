using FluentValidation;
using SmartTaskManager.Api.Authentication;
using SmartTaskManager.Api.Endpoints.Auth;
using SmartTaskManager.Application.Common.Abstractions;

namespace SmartTaskManager.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

        return services;
    }
}
