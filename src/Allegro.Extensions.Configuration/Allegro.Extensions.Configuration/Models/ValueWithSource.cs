namespace Allegro.Extensions.Configuration.Models;

/// <summary>
/// Configuration value with information about its source.
/// </summary>
/// <param name="Value">Value of the configuration key</param>
/// <param name="ProviderId">ID of the provider for this value (references ConfigurationResponse.Providers)</param>
/// <param name="ConfigurationClass">The name of the configuration class that uses this value (if any)</param>
/// <param name="AdditionalInfo">Additional info (if any) available for value</param>
public record ValueWithSource(
    string? Value,
    string ProviderId,
    string? ConfigurationClass,
    string? AdditionalInfo);