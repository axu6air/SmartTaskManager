using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Ai;

public static class AiErrors
{
    public static readonly Error Unavailable = new(
        ErrorType.ServiceUnavailable,
        "The AI service is unavailable. Try again later."
    );

    public static readonly Error InvalidResponse = new(
        ErrorType.ServiceUnavailable,
        "The AI service returned an unusable response. Try again later."
    );
}
