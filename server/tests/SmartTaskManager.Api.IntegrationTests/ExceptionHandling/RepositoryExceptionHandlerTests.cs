using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SmartTaskManager.Api.ExceptionHandling;
using SmartTaskManager.Infrastructure.Errors;
using SmartTaskManager.Infrastructure.Exceptions;

namespace SmartTaskManager.Api.IntegrationTests.ExceptionHandling;

public class RepositoryExceptionHandlerTests
{
    private static async Task<(bool Handled, int Status, string Body)> HandleAsync(Exception exception)
    {
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns("Production");
        var handler = new RepositoryExceptionHandler(environment, NullLogger<RepositoryExceptionHandler>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        context.Response.Body.Position = 0;
        return (handled, context.Response.StatusCode, await new StreamReader(context.Response.Body).ReadToEndAsync());
    }

    [Theory]
    [InlineData(RepositoryErrorType.ConcurrencyConflict, 409)]
    [InlineData(RepositoryErrorType.Deadlock, 409)]
    [InlineData(RepositoryErrorType.CommandTimeout, 503)]
    [InlineData(RepositoryErrorType.ConnectionTimeout, 503)]
    [InlineData(RepositoryErrorType.RetryExhausted, 503)]
    [InlineData(RepositoryErrorType.SqlError, 500)]
    [InlineData(RepositoryErrorType.UpdateFailed, 500)]
    public async Task TryHandleAsync_ErrorType_ReturnsMatchingStatusWithoutTechnicalDetail(
        RepositoryErrorType type, int expectedStatus)
    {
        var exception = new RepositoryException("SQL error: table dbo.Users") { ErrorType = type };

        var (handled, status, body) = await HandleAsync(exception);

        handled.Should().BeTrue();
        status.Should().Be(expectedStatus);
        body.Should().NotContain("dbo.Users");
        JsonDocument.Parse(body).RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task TryHandleAsync_UiMessage_IsSentToTheClient()
    {
        var exception = new RepositoryException("Name already used.", "technical") { ErrorType = RepositoryErrorType.UpdateFailed };

        var (_, _, body) = await HandleAsync(exception);

        body.Should().Contain("Name already used.");
    }

    [Fact]
    public async Task TryHandleAsync_OtherException_IsNotHandled()
    {
        var (handled, _, _) = await HandleAsync(new InvalidOperationException());

        handled.Should().BeFalse();
    }
}
