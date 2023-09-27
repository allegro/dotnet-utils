using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.ApplicationInsights.Prometheus;

/// <summary>
/// IServiceCollection extensions
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    /// Add export of application insights telemetry to prometheus metrics
    /// </summary>
    public static IServiceCollection AddApplicationInsightsToPrometheus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<PrometheusMetrics>();
        services.Configure<ApplicationInsightsToPrometheusMetricsConfig>(
            configuration.GetSection(ApplicationInsightsToPrometheusMetricsConfig.SectionName));
        services.AddSingleton<ITelemetryInitializer, ApplicationInsightsToPrometheusMetricsInitializer>();
        return services;
    }
}