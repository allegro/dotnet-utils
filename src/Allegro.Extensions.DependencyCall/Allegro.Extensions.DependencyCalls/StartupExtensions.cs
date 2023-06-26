using System.Reflection;
using Allegro.Extensions.DependencyCalls.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.DependencyCalls;

/// <summary>
/// Startup extensions for FluentValidation
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    /// Register dependency call abstractions and scan to register usages;
    /// </summary>
    public static IServiceCollection AddDependencyCall(
        this IServiceCollection services,
        Action<DependencyCallBuilder>? configureDependencyCall = null,
        IReadOnlyCollection<Assembly>? applicationAssemblies = null)
    {
        var builder = new DependencyCallBuilder(services);
        configureDependencyCall?.Invoke(builder);
        builder.Build();

        services.Scan(
            s => s
                .FromAssemblies(
                    applicationAssemblies ??
                    AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(c => c.AssignableTo(typeof(DependencyCall<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());

        return services;
        // TODO: generic services.AddSingleton<IDependencyCallDispatcher, DependencyCallDispatcher>();
    }
}

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