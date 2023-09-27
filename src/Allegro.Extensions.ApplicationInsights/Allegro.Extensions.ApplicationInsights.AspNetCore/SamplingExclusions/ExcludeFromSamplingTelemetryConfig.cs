namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

/// <summary>
/// Configuration for sampling exclusion ODATA rules
/// </summary>
public record ExcludeFromSamplingTelemetryConfig
{
    /// <summary>
    /// Sampling dependency exclusion ODATA rules
    /// </summary>
    public Dictionary<string, string>? DependencyRules { get; init; }

    /// <summary>
    /// Sampling request exclusion ODATA rules
    /// </summary>
    public Dictionary<string, string>? RequestRules { get; init; }
}