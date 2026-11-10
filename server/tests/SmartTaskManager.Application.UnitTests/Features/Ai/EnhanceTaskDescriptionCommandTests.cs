using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Exceptions;
using SmartTaskManager.Application.Features.Ai.Commands.EnhanceTaskDescription;

namespace SmartTaskManager.Application.UnitTests.Features.Ai;

public class EnhanceTaskDescriptionCommandTests
{
    private readonly ITaskDescriptionEnhancer _enhancer = Substitute.For<ITaskDescriptionEnhancer>();

    [Fact]
    public async Task Handle_EnhancerSucceeds_ReturnsTheSuggestion()
    {
        _enhancer.EnhanceAsync("Login", "fix login", Arg.Any<CancellationToken>())
            .Returns(new EnhancedTask("Fix login", "Fix the login flow."));

        var result = await new EnhanceTaskDescriptionCommandHandler(_enhancer)
            .Handle(new EnhanceTaskDescriptionCommand("Login", "fix login"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(new EnhanceTaskDescriptionResponse("Fix login", "Fix the login flow."));
    }

    [Fact]
    public async Task Handle_EnhancerThrows_LetsTheExceptionReachTheHandler()
    {
        _enhancer.EnhanceAsync(Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(AiServiceException.InvalidResponse());

        var act = () => new EnhanceTaskDescriptionCommandHandler(_enhancer)
            .Handle(new EnhanceTaskDescriptionCommand(null, "fix login"), default);

        await act.Should().ThrowAsync<AiServiceException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyDescription_IsInvalid(string description)
    {
        new EnhanceTaskDescriptionCommandValidator()
            .Validate(new EnhanceTaskDescriptionCommand(null, description)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_DescriptionOverColumnLength_IsInvalid()
    {
        new EnhanceTaskDescriptionCommandValidator()
            .Validate(new EnhanceTaskDescriptionCommand(null, new string('a', 4001))).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_TitleOverColumnLength_IsInvalid()
    {
        new EnhanceTaskDescriptionCommandValidator()
            .Validate(new EnhanceTaskDescriptionCommand(new string('a', 201), "fix login")).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_DescriptionWithoutTitle_IsValid()
    {
        new EnhanceTaskDescriptionCommandValidator()
            .Validate(new EnhanceTaskDescriptionCommand(null, "fix login")).IsValid.Should().BeTrue();
    }
}
