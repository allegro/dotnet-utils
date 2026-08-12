namespace Allegro.Extensions.Configuration.Models;

/// <summary>
/// Metadata of configuration provider
/// </summary>
/// <param name="DisplayName">Friendly name of the provider</param>
/// <param name="Type">Provider's type name</param>
/// <param name="Key">Optional provider-specific key, such as path to file or URL to external service</param>
/// <param name="IsSecret">Does provider hold sensitive data</param>
/// <param name="IsRawContentAvailable">Does provider expose it's raw content</param>
public record ConfigurationProviderMetadata(
    string DisplayName,
    string Type,
    string? Key,
    bool IsSecret,
    bool IsRawContentAvailable);