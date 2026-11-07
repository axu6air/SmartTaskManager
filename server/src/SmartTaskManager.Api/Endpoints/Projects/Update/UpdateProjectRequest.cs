using SmartTaskManager.Application.Features.Projects;

namespace SmartTaskManager.Api.Endpoints.Projects.Update;

public sealed record UpdateProjectRequest(
    string Name,
    string? Description,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate
) : IProjectFields;

public sealed class UpdateProjectRequestValidator : ProjectFieldsValidator<UpdateProjectRequest>;
