using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.DependencyCall.Metrics.Prometheus;

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
        builder.Services.AddSingleton(sp => new ApplicationNameProvider(applicationName));
        return builder.WithDependencyCallMetrics<PrometheusDependencyCallMetrics>();
    }
}