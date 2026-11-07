using SmartTaskManager.Domain.Exceptions;

namespace SmartTaskManager.Domain.Models.Authorization;

public class RolePermission : ITrackCreated, ITrackUpdated
{
    private const byte MaxScopeValue = 15;

    private RolePermission() { }

    internal RolePermission(Role role, Permission permission, PermissionScopes scope)
    {
        Role = role;
        RoleId = role.Id;
        Permission = permission;
        PermissionId = permission.Id;
        ChangeScope(scope);
    }

    public Guid RoleId { get; private set; }

    public Role Role { get; private set; } = null!;

    public Guid PermissionId { get; private set; }

    public Permission Permission { get; private set; } = null!;

    public PermissionScopes Scope { get; private set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public void ChangeScope(PermissionScopes scope)
    {
        if (scope == 0 || (byte)scope > MaxScopeValue)
        {
            throw new DomainException("A permission grant needs a scope between 1 and 15.");
        }

        Scope = scope;
    }
}
