using System.ClientModel;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Features.Ai;
using SmartTaskManager.Infrastructure.Ai;

namespace SmartTaskManager.Infrastructure.UnitTests.Ai;

public class OpenRouterTaskDescriptionEnhancerTests
{
    private readonly IChatClient _client = Substitute.For<IChatClient>();

    private OpenRouterTaskDescriptionEnhancer CreateEnhancer() =>
        new(_client, NullLogger<OpenRouterTaskDescriptionEnhancer>.Instance);

    private void Reply(string text) =>
        _client
            .GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ChatResponse(new ChatMessage(ChatRole.Assistant, text)));

    [Fact]
    public async Task EnhanceAsync_ValidReply_ReturnsTheTask()
    {
        Reply("""{"title": "Fix login", "description": "Fix the login flow."}""");

        var result = await CreateEnhancer().EnhanceAsync("Login", "fix login", default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(new EnhancedTask("Fix login", "Fix the login flow."));
    }

    [Fact]
    public async Task EnhanceAsync_ReplyBreaksContract_ReturnsInvalidResponse()
    {
        Reply("Sure, here you go: Fix the login flow.");

        var result = await CreateEnhancer().EnhanceAsync(null, "fix login", default);

        result.Error.Should().Be(AiErrors.InvalidResponse);
    }

    [Fact]
    public async Task EnhanceAsync_ServiceReturnsError_ReturnsUnavailable()
    {
        _client
            .GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ClientResultException("429 Too Many Requests"));

        var result = await CreateEnhancer().EnhanceAsync(null, "fix login", default);

        result.Error.Should().Be(AiErrors.Unavailable);
    }

    [Fact]
    public async Task EnhanceAsync_Timeout_ReturnsUnavailable()
    {
        _client
            .GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        var result = await CreateEnhancer().EnhanceAsync(null, "fix login", default);

        result.Error.Should().Be(AiErrors.Unavailable);
    }

    [Fact]
    public async Task EnhanceAsync_CallerCancels_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        _client
            .GetResponseAsync(Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        var act = () => CreateEnhancer().EnhanceAsync(null, "fix login", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
