using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SmartTaskManager.Api.ExceptionHandling;
using SmartTaskManager.Domain.Exceptions;

namespace SmartTaskManager.Api.IntegrationTests.ExceptionHandling;

public class GlobalExceptionHandlerTests
{
    private static async Task<(int Status, JsonElement Body)> HandleAsync(
        Exception exception,
        string environmentName = "Production"
    )
    {
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(environmentName);
        var handler = new GlobalExceptionHandler(
            environment,
            NullLogger<GlobalExceptionHandler>.Instance
        );

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.Body.Position = 0;
        var body =
            context.Response.Body.Length == 0
                ? default
                : await JsonDocument
                    .ParseAsync(context.Response.Body)
                    .ContinueWith(doc => doc.Result.RootElement.Clone());

        return (context.Response.StatusCode, body);
    }

    [Fact]
    public async Task TryHandleAsync_DomainException_Returns400WithItsMessage()
    {
        var (status, body) = await HandleAsync(
            new DomainException("A project must keep at least one manager.")
        );

        status.Should().Be(400);
        body.GetProperty("message")
            .GetString()
            .Should()
            .Be("A project must keep at least one manager.");
        body.GetProperty("success").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task TryHandleAsync_UnknownException_Returns500WithoutLeakingDetails()
    {
        var (status, body) = await HandleAsync(
            new InvalidOperationException("secret connection string")
        );

        status.Should().Be(500);
        body.GetProperty("message").GetString().Should().Be("An unexpected error occurred.");
        body.ToString().Should().NotContain("secret connection string");
    }

    [Fact]
    public async Task TryHandleAsync_UnknownExceptionInDevelopment_IncludesTheStackTrace()
    {
        var (_, body) = await HandleAsync(new InvalidOperationException("boom"), "Development");

        body.GetProperty("errors").GetProperty("exception")[0].GetString().Should().Contain("boom");
    }
}
