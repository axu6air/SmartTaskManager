using System.ClientModel;
using FluentAssertions;
using Microsoft.Extensions.AI;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Exceptions;
using SmartTaskManager.Infrastructure.Ai;

namespace SmartTaskManager.Infrastructure.UnitTests.Ai;

public class OpenRouterTaskDescriptionEnhancerTests
{
    private readonly IChatClient _client = Substitute.For<IChatClient>();

    private OpenRouterTaskDescriptionEnhancer CreateEnhancer() => new(_client);

    private void Reply(string text) =>
        _client
            .GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ChatResponse(new ChatMessage(ChatRole.Assistant, text)));

    private void Throw(Exception exception) =>
        _client
            .GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

    [Fact]
    public async Task EnhanceAsync_ValidReply_ReturnsTheTask()
    {
        Reply("""{"title": "Fix login", "description": "Fix the login flow."}""");

        var task = await CreateEnhancer().EnhanceAsync("Login", "fix login", default);

        task.Should().Be(new EnhancedTask("Fix login", "Fix the login flow."));
    }

    [Fact]
    public async Task EnhanceAsync_ReplyBreaksContract_ThrowsInvalidResponse()
    {
        Reply("Sure, here you go: Fix the login flow.");

        var act = () => CreateEnhancer().EnhanceAsync(null, "fix login", default);

        (await act.Should().ThrowAsync<AiServiceException>()).Which.ErrorType.Should().Be(AiErrorType.InvalidResponse);
    }

    [Fact]
    public async Task EnhanceAsync_ServiceReturnsError_ThrowsUnavailableWithInnerException()
    {
        var error = new ClientResultException("429 Too Many Requests");
        Throw(error);

        var act = () => CreateEnhancer().EnhanceAsync(null, "fix login", default);

        var thrown = (await act.Should().ThrowAsync<AiServiceException>()).Which;
        thrown.ErrorType.Should().Be(AiErrorType.Unavailable);
        thrown.InnerException.Should().BeSameAs(error);
    }

    [Fact]
    public async Task EnhanceAsync_ServiceUnreachable_ThrowsUnavailable()
    {
        Throw(new HttpRequestException("No such host"));

        var act = () => CreateEnhancer().EnhanceAsync(null, "fix login", default);

        (await act.Should().ThrowAsync<AiServiceException>()).Which.ErrorType.Should().Be(AiErrorType.Unavailable);
    }

    [Fact]
    public async Task EnhanceAsync_Timeout_ThrowsTimeout()
    {
        Throw(new TaskCanceledException());

        var act = () => CreateEnhancer().EnhanceAsync(null, "fix login", default);

        (await act.Should().ThrowAsync<AiServiceException>()).Which.ErrorType.Should().Be(AiErrorType.Timeout);
    }

    [Fact]
    public async Task EnhanceAsync_CallerCancels_ThrowsOperationCanceled()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        Throw(new OperationCanceledException(cts.Token));

        var act = () => CreateEnhancer().EnhanceAsync(null, "fix login", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
