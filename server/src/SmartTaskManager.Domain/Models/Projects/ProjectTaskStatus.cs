namespace SmartTaskManager.Domain.Models.Projects;

public enum ProjectTaskStatus : byte
{
    ToDo = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
}
