namespace SmartTaskManager.Domain;

public interface ITrackCreated
{
    Guid? CreatedBy { get; set; }

    DateTimeOffset CreatedAt { get; set; }
}
