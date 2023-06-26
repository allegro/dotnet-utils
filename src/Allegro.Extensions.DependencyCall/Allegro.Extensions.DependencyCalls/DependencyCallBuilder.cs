using Allegro.Extensions.DependencyCalls.Abstractions;
using Allegro.Extensions.DependencyCalls.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.DependencyCalls;

/// <summary>
/// Builder class to compose DependencyCall pipeline
/// </summary>
public sealed class DependencyCallBuilder
{
    /// <summary>
    /// Collection of services
    /// </summary>
    public IServiceCollection Services { get; }

    private Type _dependencyCallMetricsType = typeof(NoOperationDependencyCallMetrics);
    private IDependencyCallMetrics? _dependencyCallMetricsInstance;

    /// <summary>
    /// Default ctor
    /// </summary>
    public DependencyCallBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Allows to register custom IDependencyCallMetrics type
    /// </summary>
    /// <typeparam name="T">IDependencyCallMetrics</typeparam>
    public DependencyCallBuilder WithDependencyCallMetrics<T>() where T : IDependencyCallMetrics
    {
        _dependencyCallMetricsType = typeof(T);
        return this;
    }

    /// <summary>
    /// Allows to register custom IDependencyCallMetrics instance
    /// </summary>
    public DependencyCallBuilder WithDependencyCallMetrics(IDependencyCallMetrics dependencyCallMetrics)
    {
        _dependencyCallMetricsInstance = dependencyCallMetrics;
        return this;
    }

    /// <summary>
    /// Builder for with noop impl
    /// </summary>
    public void Build()
    {
        if (_dependencyCallMetricsInstance is not null)
        {
            Services.AddSingleton<IDependencyCallMetrics>(_ => _dependencyCallMetricsInstance);
        }
        else
        {
            Services.AddSingleton(typeof(IDependencyCallMetrics), _dependencyCallMetricsType);
        }
    }
}