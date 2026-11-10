using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Features.Ai;

namespace SmartTaskManager.Infrastructure.Ai;

public sealed class OpenRouterTaskDescriptionEnhancer(
    IChatClient client,
    ILogger<OpenRouterTaskDescriptionEnhancer> logger
) : ITaskDescriptionEnhancer
{
    public async Task<Result<EnhancedTask>> EnhanceAsync(
        string? title,
        string description,
        CancellationToken ct
    )
    {
        ChatResponse response;

        // Timeout and retries come from the client's options; see AiServiceCollectionExtensions.
        // The task text is never logged.
        try
        {
            response = await client.GetResponseAsync(
                [
                    new ChatMessage(ChatRole.System, TaskDescriptionPrompt.System),
                    new ChatMessage(ChatRole.User, TaskDescriptionPrompt.BuildUserMessage(title, description)),
                ],
                // A new instance per call: ChatOptions is mutable and the client pipeline may change it.
                new ChatOptions
                {
                    ResponseFormat = ChatResponseFormat.Json,
                    Temperature = 0.3f,
                    MaxOutputTokens = 1500,
                },
                ct
            );
        }
        catch (ClientResultException ex)
        {
            logger.LogWarning("AI description request failed with status {Status}.", ex.Status);
            return Result<EnhancedTask>.Failure(AiErrors.Unavailable);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "AI description request could not reach the service.");
            return Result<EnhancedTask>.Failure(AiErrors.Unavailable);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("AI description request timed out.");
            return Result<EnhancedTask>.Failure(AiErrors.Unavailable);
        }

        var improved = TaskDescriptionPrompt.ParseResponse(response.Text);

        if (improved is null)
        {
            logger.LogWarning("AI description response did not match the expected JSON contract.");
            return Result<EnhancedTask>.Failure(AiErrors.InvalidResponse);
        }

        return Result<EnhancedTask>.Success(improved);
    }
}
