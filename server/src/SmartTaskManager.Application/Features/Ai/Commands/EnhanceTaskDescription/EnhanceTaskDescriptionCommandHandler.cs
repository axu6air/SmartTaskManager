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
        // AI failures throw AiServiceException, which AiServiceExceptionHandler turns into the HTTP response.
        var task = await enhancer.EnhanceAsync(
            request.Title,
            request.Description,
            cancellationToken
        );

        return Result<EnhanceTaskDescriptionResponse>.Success(
            new EnhanceTaskDescriptionResponse(task.Title, task.Description)
        );
    }
}
