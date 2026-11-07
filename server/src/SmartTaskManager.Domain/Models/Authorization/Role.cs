using SmartTaskManager.Domain.Exceptions;

namespace SmartTaskManager.Domain.Models.Authorization;

public class Role : Entity, ITrackCreated, ITrackUpdated
{
    private readonly List<RolePermission> _rolePermissions = [];

    private Role() { }

    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsSystem { get; set; }

    public bool IsDefault { get; set; }

    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions;

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public byte[] RowVersion { get; private set; } = [];

    public static string NormalizeName(string name) => name.Trim().ToUpperInvariant();

    // System and default roles are seeded by the SQL script, so roles created here are always custom.
    public static Role Create(string name, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Role
        {
            Name = name.Trim(),
            NormalizedName = NormalizeName(name),
            Description = description,
        };
    }

    public void Update(string name, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (IsSystem)
        {
            throw new DomainException("System roles cannot be edited.");
        }

        Name = name.Trim();
        NormalizedName = NormalizeName(name);
        Description = description;
    }

    // RolePermissions must be loaded before calling Grant or Revoke.
    public void Grant(Permission permission, PermissionScopes scope)
    {
        ArgumentNullException.ThrowIfNull(permission);

        var existing = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permission.Id);
        if (existing is null)
        {
            _rolePermissions.Add(new RolePermission(this, permission, scope));
        }
        else
        {
            existing.ChangeScope(scope);
        }
    }

    public void Revoke(Guid permissionId)
    {
        var existing =
            _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId)
            ?? throw new DomainException("The role does not hold this permission.");

        _rolePermissions.Remove(existing);
    }
}
