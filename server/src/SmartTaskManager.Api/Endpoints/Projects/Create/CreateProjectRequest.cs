using SmartTaskManager.Application.Features.Projects;

namespace SmartTaskManager.Api.Endpoints.Projects.Create;

public sealed record CreateProjectRequest(
    string Name,
    string? Description,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate
) : IProjectFields;

// The field rules are shared with the command validators, so they have one definition.
public sealed class CreateProjectRequestValidator : ProjectFieldsValidator<CreateProjectRequest>;
