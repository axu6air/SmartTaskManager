namespace SmartTaskManager.Domain;

public interface ITrackUpdated
{
    Guid? UpdatedBy { get; set; }

    DateTimeOffset? UpdatedAt { get; set; }
}
