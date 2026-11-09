using FluentAssertions;
using NSubstitute;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Features.Auth.Commands.Refresh;
using SmartTaskManager.Domain.Models.Identity;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.UnitTests.Features.Auth;

public class RefreshTokenCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly TimeProvider _time = Substitute.For<TimeProvider>();
    private readonly User _user = User.Create("Member", "member@email.com", "hash");
    private readonly Guid _familyId = Guid.NewGuid();

    private RefreshTokenCommandHandler CreateHandler(RefreshToken? stored)
    {
        _time.GetUtcNow().Returns(Now);
        _refreshTokens.FindAsync(default!, true, default, true, default).ReturnsForAnyArgs(stored);
        _users.FindAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(_user);
        _users.FindAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(_user);
        _tokens.HashRefreshToken("old").Returns("old-hash");
        _tokens.HashRefreshToken("new").Returns("new-hash");
        _tokens.CreateRefreshToken().Returns("new");
        _tokens.CreateAccessToken(_user).Returns("access");
        _tokens.AccessTokenLifetime.Returns(TimeSpan.FromMinutes(15));
        _tokens.RefreshTokenLifetime.Returns(TimeSpan.FromDays(7));

        return new RefreshTokenCommandHandler(_users, _refreshTokens, _tokens, _time);
    }

    private RefreshToken Stored(DateTimeOffset expiresAt) =>
        RefreshToken.Create(_user.Id, "old-hash", _familyId, expiresAt, "127.0.0.1");

    private static RefreshTokenCommand Command => new("old", "127.0.0.1");

    [Fact]
    public async Task Handle_ActiveToken_RotatesWithinTheSameFamily()
    {
        var stored = Stored(Now.AddDays(1));

        var result = await CreateHandler(stored).Handle(Command, default);

        result.Error.Should().BeNull();
        result.Value!.RefreshToken.Should().Be("new");
        stored.RevokeReason.Should().Be(RefreshTokenRevokeReasons.Rotated);
        await _refreshTokens.Received(1).CreateAsync(
            Arg.Is<RefreshToken>(t => t.TokenHash == "new-hash" && t.FamilyId == _familyId),
            false,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RevokedToken_RevokesTheWholeFamilyAndReturnsUnauthorized()
    {
        var stored = Stored(Now.AddDays(1));
        stored.Revoke(Now.AddMinutes(-5), RefreshTokenRevokeReasons.Rotated, null);
        var sibling = Stored(Now.AddDays(1));
        var handler = CreateHandler(stored);
        _refreshTokens.FindAllAsync(default!, true, default, true, default).ReturnsForAnyArgs([sibling]);

        var result = await handler.Handle(Command, default);

        result.Error!.Type.Should().Be(ErrorType.Unauthorized);
        sibling.RevokeReason.Should().Be(RefreshTokenRevokeReasons.ReuseDetected);
        await _refreshTokens.DidNotReceiveWithAnyArgs().CreateAsync(default!, default(bool), default);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsUnauthorized()
    {
        var result = await CreateHandler(Stored(Now.AddSeconds(-1))).Handle(Command, default);

        result.Error!.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_UnknownToken_ReturnsUnauthorized()
    {
        var result = await CreateHandler(null).Handle(Command, default);

        result.Error!.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_InactiveUser_RevokesTokenAndReturnsUnauthorized()
    {
        var stored = Stored(Now.AddDays(1));
        _user.Deactivate();

        var result = await CreateHandler(stored).Handle(Command, default);

        result.Error!.Type.Should().Be(ErrorType.Unauthorized);
        stored.IsRevoked.Should().BeTrue();
    }
}
