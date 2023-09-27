namespace Allegro.Extensions.ApplicationInsights.Prometheus;

/// <summary>
/// Configuration class for ApplicationInsightsToPrometheusMetrics mechanism
/// </summary>
public class ApplicationInsightsToPrometheusMetricsConfig
{
    /// <summary>
    /// Section path for this configuration class
    /// </summary>
    public const string SectionName = "ApplicationInsights:ApplicationInsightsToPrometheusMetrics";

    /// <summary>
    /// Which dependency types should be exported
    /// </summary>
    public List<string> DependenciesTypesIncluded { get; set; } = new();

    /// <summary>
    /// Should include Azure Bus requests
    /// </summary>
    public bool IncludeBusRequests { get; set; }

    /// <summary>
    /// Should generalize http dependency operation name
    /// </summary>
    public bool ShouldGeneralizeHttpDependencyOperationName { get; set; }

    /// <summary>
    /// Should generalize http dependency target url
    /// </summary>
    public bool ShouldGeneralizeHttpDependencyTargetUrl { get; set; }

    /// <summary>
    /// How many uris per host can be exported before circuit break
    /// </summary>
    public int MaxUrisPerHost { get; set; } = 100;
}