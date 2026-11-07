namespace SmartTaskManager.Domain.Models.Authorization;

public class PermissionModule : Entity, ITrackCreated, ITrackUpdated
{
    private PermissionModule() { }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int DisplayOrder { get; private set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public static PermissionModule Create(
        string code,
        string name,
        string? description,
        int displayOrder
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new PermissionModule
        {
            Code = code.Trim(),
            Name = name.Trim(),
            Description = description,
            DisplayOrder = displayOrder,
        };
    }
}
