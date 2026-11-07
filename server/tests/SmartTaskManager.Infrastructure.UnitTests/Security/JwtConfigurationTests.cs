using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartTaskManager.Infrastructure.Security;

namespace SmartTaskManager.Infrastructure.UnitTests.Security;

public class JwtConfigurationTests
{
    private const string ValidKey = "0123456789abcdef0123456789abcdef";

    private static JwtConfiguration Resolve(string secretKey)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "issuer",
                ["Jwt:Audience"] = "audience",
                ["Jwt:SecretKey"] = secretKey,
            })
            .Build();
        var services = new ServiceCollection().AddSingleton<IConfiguration>(configuration);
        services.AddJwtConfiguration();

        return services.BuildServiceProvider().GetRequiredService<IOptions<JwtConfiguration>>().Value;
    }

    [Fact]
    public void Resolve_ValidSettings_BuildsSigningCredentialsFromSecretKey()
    {
        var jwt = Resolve(ValidKey);

        jwt.SigningCredentials.Should().NotBeNull();
        jwt.TokenExpiry.Should().Be(15);
    }

    [Fact]
    public void Resolve_ShortSecretKey_ThrowsValidationException()
    {
        var act = () => Resolve("too-short");

        act.Should().Throw<OptionsValidationException>();
    }
}
