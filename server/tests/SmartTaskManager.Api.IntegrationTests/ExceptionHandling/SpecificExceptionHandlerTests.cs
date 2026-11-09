using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SmartTaskManager.Api.ExceptionHandling;

namespace SmartTaskManager.Api.IntegrationTests.ExceptionHandling;

public class SpecificExceptionHandlerTests
{
    private static readonly IHostEnvironment Production = CreateEnvironment();

    private static IHostEnvironment CreateEnvironment()
    {
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns("Production");
        return environment;
    }

    private static DefaultHttpContext CreateContext(bool clientAborted = false)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        if (clientAborted)
        {
            context.RequestAborted = new CancellationToken(canceled: true);
        }

        return context;
    }

    private static JsonElement ReadBody(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        return JsonDocument.Parse(context.Response.Body).RootElement;
    }

    private static CancellationExceptionHandler Cancellation() =>
        new(Production, NullLogger<CancellationExceptionHandler>.Instance);

    private static InvalidOperationExceptionHandler InvalidOperation() =>
        new(Production, NullLogger<InvalidOperationExceptionHandler>.Instance);

    [Theory]
    [InlineData(typeof(OperationCanceledException))]
    [InlineData(typeof(TaskCanceledException))]
    public async Task Cancellation_ClientAborted_Returns499WithNoBody(Type exceptionType)
    {
        var context = CreateContext(clientAborted: true);
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;

        var handled = await Cancellation().TryHandleAsync(context, exception, CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(499);
        context.Response.Body.Length.Should().Be(0);
    }

    [Fact]
    public async Task Cancellation_AnyExceptionAfterClientLeft_Returns499()
    {
        var context = CreateContext(clientAborted: true);

        var handled = await Cancellation().TryHandleAsync(context, new IOException(), CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(499);
    }

    [Fact]
    public async Task Cancellation_InternalTimeoutWhileClientWaits_Returns504()
    {
        var context = CreateContext();

        var handled = await Cancellation().TryHandleAsync(context, new TaskCanceledException(), CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(504);
        ReadBody(context).GetProperty("success").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Cancellation_OtherException_IsNotHandled()
    {
        var handled = await Cancellation().TryHandleAsync(CreateContext(), new InvalidOperationException(), CancellationToken.None);

        handled.Should().BeFalse();
    }

    [Fact]
    public async Task InvalidOperation_Returns500WithoutLeakingTheMessage()
    {
        var context = CreateContext();

        var handled = await InvalidOperation().TryHandleAsync(
            context, new InvalidOperationException("The role 'Admin' is missing."), CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(500);
        ReadBody(context).ToString().Should().NotContain("Admin");
    }

    [Fact]
    public async Task InvalidOperation_OtherException_IsNotHandled()
    {
        var handled = await InvalidOperation().TryHandleAsync(CreateContext(), new ArgumentException(), CancellationToken.None);

        handled.Should().BeFalse();
    }

    [Fact]
    public async Task Chain_RunsTheFirstHandlerThatClaimsTheException()
    {
        IExceptionHandler[] chain =
        [
            Cancellation(),
            InvalidOperation(),
            new GlobalExceptionHandler(Production, NullLogger<GlobalExceptionHandler>.Instance),
        ];
        var context = CreateContext();
        var exception = new ArgumentException("anything else");

        foreach (var handler in chain)
        {
            if (await handler.TryHandleAsync(context, exception, CancellationToken.None))
            {
                break;
            }
        }

        context.Response.StatusCode.Should().Be(500);
        ReadBody(context).GetProperty("message").GetString().Should().Be("An unexpected error occurred.");
    }
}
