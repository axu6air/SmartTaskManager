using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Ai.Commands.EnhanceTaskDescription;

// The title is optional: it is improved when given, and written from the description when not.
public sealed record EnhanceTaskDescriptionCommand(string? Title, string Description)
    : IRequest<Result<EnhanceTaskDescriptionResponse>>;
