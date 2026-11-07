using FluentAssertions;
using NSubstitute;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Features.Auth.Commands.Login;
using SmartTaskManager.Domain.Models.Identity;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.UnitTests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly TimeProvider _time = Substitute.For<TimeProvider>();
    private readonly User _user = User.Create("Member", "member@email.com", "hash");

    private LoginCommandHandler CreateHandler(User? found, bool passwordMatches = true)
    {
        _time.GetUtcNow().Returns(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        _users.FindAsync(default!, default, true, default).ReturnsForAnyArgs(found);
        _hasher.Verify(default!, default!).ReturnsForAnyArgs(passwordMatches);
        _tokens.AccessTokenLifetime.Returns(TimeSpan.FromMinutes(15));
        _tokens.RefreshTokenLifetime.Returns(TimeSpan.FromDays(7));
        _tokens.CreateAccessToken(_user).Returns("access");
        _tokens.CreateRefreshToken().Returns("refresh");
        _tokens.HashRefreshToken("refresh").Returns("refresh-hash");

        return new LoginCommandHandler(_users, _refreshTokens, _hasher, _tokens, _time);
    }

    private static LoginCommand Command => new("member@email.com", "password123", "127.0.0.1");

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokensAndStoresRefreshTokenHash()
    {
        var result = await CreateHandler(_user).Handle(Command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access");
        result.Value.RefreshToken.Should().Be("refresh");
        result.Value.AccessTokenExpiresAt.Should().Be(_time.GetUtcNow().AddMinutes(15));
        await _refreshTokens.Received(1).CreateAsync(
            Arg.Is<RefreshToken>(t => t.TokenHash == "refresh-hash" && t.UserId == _user.Id
                && t.ExpiresAt == _time.GetUtcNow().AddDays(7)),
            Arg.Any<CancellationToken>());
        _user.LastLoginAt.Should().Be(_time.GetUtcNow());
    }

    [Fact]
    public async Task Handle_UnknownEmail_ReturnsUnauthorized()
    {
        var result = await CreateHandler(null).Handle(Command, default);

        result.Error!.Type.Should().Be(ErrorType.Unauthorized);
        await _refreshTokens.DidNotReceiveWithAnyArgs().CreateAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsUnauthorizedAndCountsTheFailure()
    {
        var result = await CreateHandler(_user, passwordMatches: false).Handle(Command, default);

        result.Error!.Type.Should().Be(ErrorType.Unauthorized);
        _user.AccessFailedCount.Should().Be(1);
        await _refreshTokens.DidNotReceiveWithAnyArgs().CreateAsync(default!, default);
    }

    [Fact]
    public async Task Handle_InactiveUser_ReturnsUnauthorized()
    {
        _user.Deactivate();

        var result = await CreateHandler(_user).Handle(Command, default);

        result.Error!.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_LockedOutUser_ReturnsUnauthorizedEvenWithCorrectPassword()
    {
        var handler = CreateHandler(_user);
        _user.RecordFailedLogin(_time.GetUtcNow(), maxFailedAttempts: 1, TimeSpan.FromHours(1));

        var result = await handler.Handle(Command, default);

        result.Error!.Type.Should().Be(ErrorType.Unauthorized);
    }
}
