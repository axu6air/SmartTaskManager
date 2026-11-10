using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SmartTaskManager.Api.ExceptionHandling;
using SmartTaskManager.Application.Common.Exceptions;

namespace SmartTaskManager.Api.IntegrationTests.ExceptionHandling;

public class AiServiceExceptionHandlerTests
{
    private static async Task<(bool Handled, int Status, string Body)> HandleAsync(Exception exception)
    {
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns("Production");
        var handler = new AiServiceExceptionHandler(environment, NullLogger<AiServiceExceptionHandler>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        context.Response.Body.Position = 0;
        return (handled, context.Response.StatusCode, await new StreamReader(context.Response.Body).ReadToEndAsync());
    }

    public static TheoryData<AiServiceException, int> Failures => new()
    {
        { AiServiceException.Unavailable(new HttpRequestException("secret host detail")), 503 },
        { AiServiceException.Timeout(new TaskCanceledException("secret host detail")), 504 },
        { AiServiceException.InvalidResponse(), 502 },
    };

    [Theory]
    [MemberData(nameof(Failures))]
    public async Task TryHandleAsync_ErrorType_ReturnsMatchingStatusWithUiMessageOnly(AiServiceException exception, int expectedStatus)
    {
        var (handled, status, body) = await HandleAsync(exception);

        handled.Should().BeTrue();
        status.Should().Be(expectedStatus);
        body.Should().NotContain("secret host detail");

        var root = JsonDocument.Parse(body).RootElement;
        root.GetProperty("success").GetBoolean().Should().BeFalse();
        root.GetProperty("message").GetString().Should().Be(exception.UiMessage);
    }

    [Fact]
    public async Task TryHandleAsync_OtherException_IsNotHandled()
    {
        var (handled, _, _) = await HandleAsync(new InvalidOperationException());

        handled.Should().BeFalse();
    }
}
