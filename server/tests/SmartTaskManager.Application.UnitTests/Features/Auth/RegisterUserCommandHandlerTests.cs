using FluentAssertions;
using NSubstitute;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Features.Auth.Commands.Register;
using SmartTaskManager.Domain.Models.Authorization;
using SmartTaskManager.Domain.Models.Identity;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.UnitTests.Features.Auth;

public class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IRoleRepository _roles = Substitute.For<IRoleRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly Role _defaultRole = Role.Create("Team Member", null);
    private readonly Role _chosenRole = Role.Create("Project Manager", null);

    public RegisterUserCommandHandlerTests()
    {
        _hasher.Hash("Secret123").Returns("hashed");
        _roles.FindAsync(default!, true, default, true, default).ReturnsForAnyArgs(_defaultRole);
        _roles.FindAsync(Arg.Any<Guid>(), true, Arg.Any<CancellationToken>()).Returns(_chosenRole);
    }

    private RegisterUserCommandHandler Handler => new(_users, _roles, _hasher);

    private static RegisterUserCommand Command(Guid? roleId = null) =>
        new("New Person", "New.Person@Email.com", "Secret123", roleId);

    [Fact]
    public async Task Handle_NoRole_CreatesUserWithDefaultRoleAndHashedPassword()
    {
        var result = await Handler.Handle(Command(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Roles.Should().Equal("Team Member");
        await _users.Received(1).CreateAsync(
            Arg.Is<User>(u => u.PasswordHash == "hashed" && u.NormalizedEmail == "NEW.PERSON@EMAIL.COM"
                && u.UserRoles.Single().Role == _defaultRole),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ChosenRole_AssignsThatRole()
    {
        var result = await Handler.Handle(Command(Guid.NewGuid()), default);

        result.Value!.Roles.Should().Equal("Project Manager");
    }

    [Fact]
    public async Task Handle_EmailTaken_ReturnsConflictAndCreatesNothing()
    {
        _users.ExistsAsync(Arg.Any<System.Linq.Expressions.Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await Handler.Handle(Command(), default);

        result.Error!.Type.Should().Be(ErrorType.Conflict);
        await _users.DidNotReceiveWithAnyArgs().CreateAsync(default!, default(CancellationToken));
    }

    [Fact]
    public async Task Handle_UnknownRole_ReturnsNotFound()
    {
        _roles.FindAsync(Arg.Any<Guid>(), true, Arg.Any<CancellationToken>()).Returns((Role?)null);

        var result = await Handler.Handle(Command(Guid.NewGuid()), default);

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Theory]
    [InlineData("short1A")]
    [InlineData("alllowercase1")]
    [InlineData("ALLUPPERCASE1")]
    [InlineData("NoDigitsHere")]
    public void Validator_WeakPassword_IsRejected(string password)
    {
        var result = new RegisterUserCommandValidator().Validate(new RegisterUserCommand("A", "a@b.com", password, null));

        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }
}
