using System.ClientModel;
using Microsoft.Extensions.AI;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Exceptions;

namespace SmartTaskManager.Infrastructure.Ai;

public sealed class OpenRouterTaskDescriptionEnhancer(IChatClient client) : ITaskDescriptionEnhancer
{
    public async Task<EnhancedTask> EnhanceAsync(
        string? title,
        string description,
        CancellationToken ct
    )
    {
        ChatResponse response;

        // Timeout and retries come from the client's options; see AiServiceCollectionExtensions.
        // Failures are logged by AiServiceExceptionHandler; the task text is never logged.
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
            throw AiServiceException.Unavailable(ex);
        }
        catch (HttpRequestException ex)
        {
            throw AiServiceException.Unavailable(ex);
        }
        catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
        {
            // The caller did not cancel, so the client's network timeout fired.
            throw AiServiceException.Timeout(ex);
        }

        return TaskDescriptionPrompt.ParseResponse(response.Text) ?? throw AiServiceException.InvalidResponse();
    }
}
