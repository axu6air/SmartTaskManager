using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using SmartTaskManager.Api.Cors;
using SmartTaskManager.Api.RateLimiting;

namespace SmartTaskManager.Api.IntegrationTests.Configuration;

public class CrossCuttingSettingsTests
{
    private static bool IsValid(object options) =>
        Validator.TryValidateObject(options, new ValidationContext(options), [], validateAllProperties: true);

    [Theory]
    [InlineData("http://localhost:4200")]
    [InlineData("https://app.example.com")]
    public void CorsSettings_ExactOrigin_IsValid(string origin)
    {
        IsValid(new CorsSettings { AllowedOrigins = [origin] }).Should().BeTrue();
    }

    [Fact]
    public void CorsSettings_NoOrigins_IsValid()
    {
        IsValid(new CorsSettings()).Should().BeTrue();
    }

    [Theory]
    [InlineData("*")]
    [InlineData("https://*.example.com")]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com")]
    [InlineData("https://example.com/app")]
    public void CorsSettings_WildcardOrNonOrigin_IsInvalid(string origin)
    {
        IsValid(new CorsSettings { AllowedOrigins = ["https://ok.example.com", origin] }).Should().BeFalse();
    }

    [Fact]
    public void RateLimitingSettings_Defaults_AreValid()
    {
        IsValid(new RateLimitingSettings()).Should().BeTrue();
    }

    [Fact]
    public void RateLimitingSettings_ZeroPermitLimit_IsInvalid()
    {
        IsValid(new RateLimitingSettings { AiPermitLimit = 0 }).Should().BeFalse();
    }

    [Fact]
    public void GetPartitionKey_AuthenticatedUser_UsesUserId()
    {
        var userId = Guid.NewGuid();
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())], "Bearer")
            ),
        };
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");

        RateLimitingExtensions.GetPartitionKey(context).Should().Be($"user:{userId}");
    }

    [Fact]
    public void GetPartitionKey_Anonymous_UsesIpAddress()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");

        RateLimitingExtensions.GetPartitionKey(context).Should().Be("ip:10.0.0.1");
    }

    [Fact]
    public void GetPartitionKey_AnonymousWithoutIp_UsesUnknown()
    {
        RateLimitingExtensions.GetPartitionKey(new DefaultHttpContext()).Should().Be("ip:unknown");
    }
}
