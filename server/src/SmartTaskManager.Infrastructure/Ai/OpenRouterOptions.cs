using System.ComponentModel.DataAnnotations;

namespace SmartTaskManager.Infrastructure.Ai;

// OpenRouter speaks the OpenAI API, so the OpenAI SDK talks to it through BaseUrl.
public sealed class OpenRouterOptions
{
    public const string SectionName = "OpenRouter";

    [Required]
    public string ApiKey { get; init; } = string.Empty;

    [Required, Url]
    public string BaseUrl { get; init; } = "https://openrouter.ai/api/v1";

    [Required]
    public string ChatModel { get; init; } = "nvidia/nemotron-3.5-lightning:free";

    // Per attempt.
    [Range(1, 120)]
    public int TimeoutSeconds { get; init; } = 30;

    // Retries of transient failures (429, 5xx, network errors), on top of the first attempt.
    [Range(0, 5)]
    public int MaxRetries { get; init; } = 2;
}
