namespace SmartTaskManager.Api.Endpoints.Tasks.Assign;

// A null AssigneeId unassigns the task.
public sealed record AssignTaskRequest(Guid? AssigneeId);
