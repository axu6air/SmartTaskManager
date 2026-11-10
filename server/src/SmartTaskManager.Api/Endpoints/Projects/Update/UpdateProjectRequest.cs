using SmartTaskManager.Application.Features.Projects;

namespace SmartTaskManager.Api.Endpoints.Projects.Update;

public sealed record UpdateProjectRequest(
    string Name,
    string? Description,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    // Set to true when the saved content came from the AI suggestion; false when omitted.
    bool IsAiModified = false
) : IProjectFields;

public sealed class UpdateProjectRequestValidator : ProjectFieldsValidator<UpdateProjectRequest>;
