using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace SmartTaskManager.Infrastructure.Security;

public static class JwtServiceCollectionExtensions
{
    public static IServiceCollection AddJwtConfiguration(this IServiceCollection services)
    {
        services.AddOptions<JwtConfiguration>()
            .BindConfiguration(JwtConfiguration.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.PostConfigure<JwtConfiguration>(jwt =>
        {
            jwt.SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
                SecurityAlgorithms.HmacSha256);
        });

        return services;
    }
}
