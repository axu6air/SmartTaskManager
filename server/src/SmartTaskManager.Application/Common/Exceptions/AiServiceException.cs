using SmartTaskManager.Domain.Exceptions;

namespace SmartTaskManager.Application.Common.Exceptions;

public enum AiErrorType
{
    // The AI service returned an error or could not be reached.
    Unavailable = 1,

    // The AI service did not answer in time.
    Timeout = 2,

    // The AI service answered, but not in the agreed format.
    InvalidResponse = 3,
}

// Thrown by AI service implementations. The UiMessage is safe to show; the technical message
// and inner exception stay in the log. It never carries the user's task text.
public sealed class AiServiceException : BaseException
{
    public AiServiceException(AiErrorType errorType, string uiMessage, string message)
        : base(uiMessage, message)
    {
        ErrorType = errorType;
    }

    public AiServiceException(AiErrorType errorType, string uiMessage, string message, Exception innerException)
        : base(innerException, uiMessage, message)
    {
        ErrorType = errorType;
    }

    public AiErrorType ErrorType { get; }

    public static AiServiceException Unavailable(Exception innerException) =>
        new(
            AiErrorType.Unavailable,
            "The AI service is unavailable. Try again later.",
            "The AI service request failed.",
            innerException
        );

    public static AiServiceException Timeout(Exception innerException) =>
        new(
            AiErrorType.Timeout,
            "The AI service took too long to respond. Try again.",
            "The AI service request timed out.",
            innerException
        );

    public static AiServiceException InvalidResponse() =>
        new(
            AiErrorType.InvalidResponse,
            "The AI service returned an unusable response. Try again.",
            "The AI service response did not match the expected JSON contract."
        );

    public static AiServiceException InvalidResponse(Exception innerException) =>
        new(
            AiErrorType.InvalidResponse,
            "The AI service returned an unusable response. Try again.",
            "The AI service response could not be parsed.",
            innerException
        );
}
