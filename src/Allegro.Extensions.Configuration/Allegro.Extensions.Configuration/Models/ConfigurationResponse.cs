namespace Allegro.Extensions.Configuration.Models;

/// <summary>
/// Response with service's configuration
/// </summary>
/// <param name="Configuration">
/// Configuration keys with all available values from all providers. When more than one value per key,
/// the values are sorted by decreasing priority. The first value is the one accessible from IConfiguration.
/// </param>
/// <param name="Providers">
/// All configuration providers used by the service
/// </param>
public record ConfigurationResponse(
    IDictionary<string, IList<ValueWithSource>> Configuration,
    IDictionary<string, ConfigurationProviderMetadata> Providers);