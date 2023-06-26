using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.DependencyCalls.Metrics.Prometheus;

/// <summary>
/// Register Prometheus metrics extensions
/// </summary>
public static class DependencyCallBuilderExtensions
{
    /// <summary>
    /// Registers PrometheusDependencyCallMetrics into DependencyCall pipeline
    /// </summary>
    public static DependencyCallBuilder RegisterPrometheusDependencyCallMetrics(
        this DependencyCallBuilder builder,
        string applicationName)
    {
        builder.Services.AddSingleton(_ => new ApplicationNameProvider(applicationName));
        return builder.WithDependencyCallMetrics<PrometheusDependencyCallMetrics>();
    }
}