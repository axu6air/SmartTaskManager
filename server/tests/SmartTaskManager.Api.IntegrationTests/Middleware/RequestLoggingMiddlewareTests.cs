using System.Net;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SmartTaskManager.Api.Middleware;

namespace SmartTaskManager.Api.IntegrationTests.Middleware;

public class RequestLoggingMiddlewareTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 6, 1, 12, 30, 0, TimeSpan.Zero);
    }

    private sealed class ListLogger : ILogger<RequestLoggingMiddleware>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => Messages.Add(formatter(state, exception));
    }

    private static DefaultHttpContext CreateContext(bool authenticated)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/projects";
        context.Request.QueryString = new QueryString("?access_token=secret");
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.7");
        if (authenticated)
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, UserId.ToString())], "test"));
        }

        return context;
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatedRequest_LogsWhoWhenAndTheOutcome()
    {
        var logger = new ListLogger();
        var middleware = new RequestLoggingMiddleware(
            ctx =>
            {
                ctx.Response.StatusCode = 200;
                return Task.CompletedTask;
            },
            new FixedTimeProvider(),
            logger);

        await middleware.InvokeAsync(CreateContext(authenticated: true));

        logger.Messages.Should().ContainSingle().Which.Should()
            .Contain("GET /api/projects")
            .And.Contain(UserId.ToString())
            .And.Contain("10.0.0.7")
            .And.Contain("2026-06-01T12:30:00.0000000+00:00")
            .And.Contain("returned 200");
    }

    [Fact]
    public async Task InvokeAsync_AnonymousRequest_LogsAnonymousAndNeverTheQueryString()
    {
        var logger = new ListLogger();
        var middleware = new RequestLoggingMiddleware(_ => Task.CompletedTask, new FixedTimeProvider(), logger);

        await middleware.InvokeAsync(CreateContext(authenticated: false));

        logger.Messages.Single().Should().Contain("anonymous").And.NotContain("secret");
    }

    [Fact]
    public async Task InvokeAsync_NextThrows_LogsA500AndRethrows()
    {
        var logger = new ListLogger();
        var middleware = new RequestLoggingMiddleware(
            _ => throw new InvalidOperationException("boom"), new FixedTimeProvider(), logger);

        var act = () => middleware.InvokeAsync(CreateContext(authenticated: true));

        await act.Should().ThrowAsync<InvalidOperationException>();
        logger.Messages.Single().Should().Contain("returned 500");
    }
}
