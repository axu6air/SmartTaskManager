using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Ai.Commands.EnhanceTaskDescription;

public sealed class EnhanceTaskDescriptionCommandHandler(ITaskDescriptionEnhancer enhancer)
    : IRequestHandler<EnhanceTaskDescriptionCommand, Result<EnhanceTaskDescriptionResponse>>
{
    public async Task<Result<EnhanceTaskDescriptionResponse>> Handle(
        EnhanceTaskDescriptionCommand request,
        CancellationToken cancellationToken
    )
    {
        var result = await enhancer.EnhanceAsync(
            request.Title,
            request.Description,
            cancellationToken
        );

        return result.Map(task => new EnhanceTaskDescriptionResponse(task.Title, task.Description));
    }
}
