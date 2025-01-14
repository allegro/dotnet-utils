using System.Reflection;
using Allegro.Extensions.DependencyCall.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.DependencyCall;

/// <summary>
/// Startup extensions for FluentValidation
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    /// Register dependency call abstractions and scan to register usages;
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureDependencyCall">An optional action to configure the dependency call builder.</param>
    /// <param name="applicationAssemblies">An optional collection of assemblies in which dependency call types should be looked for.</param>
    /// <param name="publicOnly">Determines whether only public classes should be registered</param>
    public static IServiceCollection AddDependencyCall(
        this IServiceCollection services,
        Action<DependencyCallBuilder>? configureDependencyCall = null,
        IReadOnlyCollection<Assembly>? applicationAssemblies = null,
        bool publicOnly = false)
    {
        var builder = new DependencyCallBuilder(services);
        configureDependencyCall?.Invoke(builder);
        builder.Build();

        services.Scan(
            s => s
                .FromAssemblies(
                    applicationAssemblies ??
                    AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(c => c.AssignableTo(typeof(IDependencyCall<,>)), publicOnly)
                .AsImplementedInterfaces()// TODO: remove scrutor and register by own util
                .WithTransientLifetime());
        return services
            .AddSingleton<IDependencyCallDispatcher, DefaultDependencyCallDispatcher>();
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

    internal void Build()
    {
        if (_dependencyCallMetricsInstance is not null)
        {
            Services.AddSingleton<IDependencyCallMetrics>(sp => _dependencyCallMetricsInstance);
        }
        else
        {
            Services.AddSingleton(typeof(IDependencyCallMetrics), _dependencyCallMetricsType);
        }
    }
}