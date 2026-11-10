using System.ComponentModel.DataAnnotations;

namespace SmartTaskManager.Api.Cors;

public sealed class CorsSettings : IValidatableObject
{
    public const string SectionName = "Cors";

    // Exact origins such as "https://app.example.com". Empty means no cross-origin access.
    public string[] AllowedOrigins { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var origin in AllowedOrigins)
        {
            if (origin.Contains('*'))
            {
                yield return new ValidationResult(
                    $"Wildcard origins are not allowed: '{origin}'.",
                    [nameof(AllowedOrigins)]
                );
            }
            else if (
                !Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                || uri.AbsolutePath != "/"
            )
            {
                yield return new ValidationResult(
                    $"'{origin}' is not an origin. Use scheme, host and optional port, e.g. https://app.example.com.",
                    [nameof(AllowedOrigins)]
                );
            }
        }
    }
}
