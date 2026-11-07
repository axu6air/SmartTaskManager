namespace SmartTaskManager.Domain.Models.Auditing;

// Append-only: UpdatedBy and UpdatedAt exist for the schema convention and always stay null.
public class AuditLog : Entity, ITrackCreated, ITrackUpdated
{
    private AuditLog() { }

    public string Action { get; private set; } = string.Empty;

    public string EntityType { get; private set; } = string.Empty;

    public string EntityId { get; private set; } = string.Empty;

    public string? OldValues { get; private set; }

    public string? NewValues { get; private set; }

    public string? IpAddress { get; private set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public static AuditLog Create(
        string action,
        string entityType,
        string entityId,
        string? oldValues,
        string? newValues,
        string? ipAddress
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityId);

        return new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
        };
    }
}
