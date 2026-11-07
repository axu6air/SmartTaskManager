namespace SmartTaskManager.Domain.Models.Authorization;

public class Permission : Entity, ITrackCreated, ITrackUpdated
{
    private Permission() { }

    public Guid ModuleId { get; private set; }

    public PermissionModule Module { get; private set; } = null!;

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsScopable { get; private set; }

    public int DisplayOrder { get; private set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public static Permission Create(
        PermissionModule module,
        string code,
        string name,
        string? description,
        bool isScopable,
        int displayOrder
    )
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Permission
        {
            Module = module,
            ModuleId = module.Id,
            Code = code.Trim(),
            Name = name.Trim(),
            Description = description,
            IsScopable = isScopable,
            DisplayOrder = displayOrder,
        };
    }
}
