namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

/// <summary>
/// Configuration for sampling
/// </summary>
public record SamplingConfig
{
    /// <summary>
    /// Allow to set sampling mode
    /// </summary>
    public SamplingMode SamplingMode { get; init; }

    /// <summary>
    /// Sampling percentage for fixed mode
    /// </summary>
    public double? FixedSamplingPercentage { get; init; }

    /// <summary>
    /// A semi-colon delimited list of types that you don't want to be subject to sampling.
    /// Recognized types are: Dependency, Event, Exception, PageView, Request, Trace. All telemetry of the specified types is transmitted; the types that aren't specified will be sampled.
    /// </summary>
    public string SamplingExcludedTypes { get; init; } = "Event;Exception;Trace;PageView";

    /// <summary>
    /// Advanced configuration for adaptive sampling
    /// </summary>
    public AdaptiveSamplingConfig AdaptiveSamplingConfig { get; init; } = new AdaptiveSamplingConfig();
}