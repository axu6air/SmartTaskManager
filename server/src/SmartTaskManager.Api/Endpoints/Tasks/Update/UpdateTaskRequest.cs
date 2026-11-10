using SmartTaskManager.Application.Features.Tasks;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Api.Endpoints.Tasks.Update;

public sealed record UpdateTaskRequest(
    string Title,
    string? Description,
    ProjectTaskPriority Priority,
    DateTimeOffset? DueDate,
    // Set to true when the saved content came from the AI suggestion; false when omitted.
    bool IsAiModified = false
) : ITaskFields;

public sealed class UpdateTaskRequestValidator : TaskFieldsValidator<UpdateTaskRequest>;
