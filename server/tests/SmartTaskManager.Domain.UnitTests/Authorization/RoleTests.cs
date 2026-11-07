using FluentAssertions;
using SmartTaskManager.Domain.Exceptions;
using SmartTaskManager.Domain.Models.Authorization;

namespace SmartTaskManager.Domain.UnitTests.Authorization;

public class RoleTests
{
    private static Permission NewPermission()
    {
        var module = PermissionModule.Create("tasks", "Task Management", null, 1);
        return Permission.Create(module, "tasks.update", "Update Task", null, true, 1);
    }

    [Fact]
    public void Create_ValidInput_NormalizesName()
    {
        var role = Role.Create("  Team Lead ", null);

        role.Name.Should().Be("Team Lead");
        role.NormalizedName.Should().Be("TEAM LEAD");
        role.IsSystem.Should().BeFalse();
    }

    [Fact]
    public void Grant_SamePermissionTwice_KeepsOneGrantWithLatestScope()
    {
        var role = Role.Create("Team Lead", null);
        var permission = NewPermission();

        role.Grant(permission, PermissionScopes.Assigned);
        role.Grant(permission, PermissionScopes.Managed | PermissionScopes.Member);

        role.RolePermissions.Should()
            .ContainSingle()
            .Which.Scope.Should()
            .Be(PermissionScopes.Managed | PermissionScopes.Member);
    }

    [Fact]
    public void Grant_ScopeZero_Throws()
    {
        var role = Role.Create("Team Lead", null);

        var act = () => role.Grant(NewPermission(), 0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Revoke_PermissionNotGranted_Throws()
    {
        var role = Role.Create("Team Lead", null);

        var act = () => role.Revoke(Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }
}
