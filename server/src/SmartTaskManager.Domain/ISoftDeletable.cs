namespace SmartTaskManager.Domain;

public interface ISoftDeletable
{
    bool IsDeleted { get; }

    void MarkDeleted(Guid? deletedBy, DateTimeOffset deletedAt);
}
