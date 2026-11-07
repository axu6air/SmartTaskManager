namespace SmartTaskManager.Api.Endpoints.Projects.Assign;

public sealed record AssignProjectMemberRequest(Guid UserId, bool IsManager);
