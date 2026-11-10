using System.ClientModel;
using System.ClientModel.Primitives;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI;
using SmartTaskManager.Application.Common.Abstractions;

namespace SmartTaskManager.Infrastructure.Ai;

public static class AiServiceCollectionExtensions
{
    public static IServiceCollection AddAi(this IServiceCollection services)
    {
        services
            .AddOptions<OpenRouterOptions>()
            .BindConfiguration(OpenRouterOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // OpenRouter speaks the OpenAI API, so the OpenAI client is pointed at it and exposed as IChatClient.
        services.AddChatClient(serviceProvider =>
        {
            var openRouter = serviceProvider.GetRequiredService<IOptions<OpenRouterOptions>>().Value;

            var client = new OpenAIClient(
                new ApiKeyCredential(openRouter.ApiKey),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(openRouter.BaseUrl),
                    NetworkTimeout = TimeSpan.FromSeconds(openRouter.TimeoutSeconds),
                    RetryPolicy = new ClientRetryPolicy(openRouter.MaxRetries),
                }
            );

            return client.GetChatClient(openRouter.ChatModel).AsIChatClient();
        });

        services.AddSingleton<ITaskDescriptionEnhancer, OpenRouterTaskDescriptionEnhancer>();

        return services;
    }
}
