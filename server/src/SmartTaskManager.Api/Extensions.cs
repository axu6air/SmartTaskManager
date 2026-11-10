using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SmartTaskManager.Api.Authentication;
using SmartTaskManager.Api.ExceptionHandling;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Infrastructure.Security;

namespace SmartTaskManager.Api;

public static class ApiExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        // Configured through options so the validated JwtConfiguration is the single source of the settings.
        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtConfiguration>>(
                (bearer, jwtOptions) =>
                {
                    var jwt = jwtOptions.Value;

                    bearer.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwt.Audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = jwt.SigningCredentials!.Key,
                        ClockSkew = TimeSpan.FromSeconds(30),
                    };
                }
            );

        return services;
    }

    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        // "sub" is mapped to NameIdentifier by default, so accept either.
        var value =
            user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (user.Identity?.IsAuthenticated == true && Guid.TryParse(value, out var userId))
        {
            return userId;
        }

        return null;
    }

    public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        // Handlers run in registration order, so the specific ones come before the catch-all.
        services.AddExceptionHandler<CancellationExceptionHandler>();
        services.AddExceptionHandler<InvalidOperationExceptionHandler>();
        services.AddExceptionHandler<RepositoryExceptionHandler>();
        services.AddExceptionHandler<AiServiceExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    public static IServiceCollection AddRequestContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }
}
