using FluentAssertions;
using NSubstitute;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Features.Roles.Commands.GrantRolePermission;
using SmartTaskManager.Application.Features.Roles.Commands.RevokeRolePermission;
using SmartTaskManager.Domain.Models.Authorization;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.UnitTests.Features.Roles;

public class RolePermissionCommandHandlerTests
{
    private readonly IRoleRepository _roles = Substitute.For<IRoleRepository>();
    private readonly IPermissionRepository _permissions = Substitute.For<IPermissionRepository>();
    private readonly Role _role = Role.Create("Reviewer", null);
    private readonly PermissionModule _module = PermissionModule.Create("tasks", "Tasks", null, 1);

    private Permission Setup(bool isScopable)
    {
        var permission = Permission.Create(_module, "tasks.view", "View Task", null, isScopable, 1);
        _roles.FindWithPermissionsAsync(default, default).ReturnsForAnyArgs(_role);
        _permissions.FindAsync(default(Guid), true, default).ReturnsForAnyArgs(permission);
        return permission;
    }

    [Fact]
    public async Task Grant_CustomRole_AddsGrantAndSaves()
    {
        var permission = Setup(isScopable: true);

        var result = await new GrantRolePermissionCommandHandler(_roles, _permissions)
            .Handle(new GrantRolePermissionCommand(_role.Id, permission.Id, PermissionScopes.Member), default);

        result.IsSuccess.Should().BeTrue();
        _role.RolePermissions.Should().ContainSingle(rp => rp.Scope == PermissionScopes.Member);
        await _roles.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Grant_SystemRole_ReturnsConflict()
    {
        var permission = Setup(isScopable: true);
        _role.IsSystem = true;

        var result = await new GrantRolePermissionCommandHandler(_roles, _permissions)
            .Handle(new GrantRolePermissionCommand(_role.Id, permission.Id, PermissionScopes.All), default);

        result.Error!.Type.Should().Be(ErrorType.Conflict);
        _role.RolePermissions.Should().BeEmpty();
    }

    [Fact]
    public async Task Grant_UnscopablePermissionWithNarrowScope_ReturnsValidation()
    {
        var permission = Setup(isScopable: false);

        var result = await new GrantRolePermissionCommandHandler(_roles, _permissions)
            .Handle(new GrantRolePermissionCommand(_role.Id, permission.Id, PermissionScopes.Assigned), default);

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Revoke_HeldGrant_RemovesIt()
    {
        var permission = Setup(isScopable: true);
        _role.Grant(permission, PermissionScopes.All);

        var result = await new RevokeRolePermissionCommandHandler(_roles)
            .Handle(new RevokeRolePermissionCommand(_role.Id, permission.Id), default);

        result.IsSuccess.Should().BeTrue();
        _role.RolePermissions.Should().BeEmpty();
    }

    [Fact]
    public async Task Revoke_GrantNotHeld_ReturnsNotFound()
    {
        Setup(isScopable: true);

        var result = await new RevokeRolePermissionCommandHandler(_roles)
            .Handle(new RevokeRolePermissionCommand(_role.Id, Guid.NewGuid()), default);

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Revoke_SystemRole_ReturnsConflict()
    {
        Setup(isScopable: true);
        _role.IsSystem = true;

        var result = await new RevokeRolePermissionCommandHandler(_roles)
            .Handle(new RevokeRolePermissionCommand(_role.Id, Guid.NewGuid()), default);

        result.Error!.Type.Should().Be(ErrorType.Conflict);
    }
}
