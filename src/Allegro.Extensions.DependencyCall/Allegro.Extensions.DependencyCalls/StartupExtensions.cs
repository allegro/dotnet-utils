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
                .AddClasses(c => c.AssignableTo(typeof(IDependencyCall<,>)))
                .AsImplementedInterfaces() // in case of using AsyncCircuitBreakerPolicy remember that transient will not work
                .WithTransientLifetime());

        return services
            .AddSingleton<IDependencyCallDispatcher, DefaultDependencyCallDispatcher>();
    }
}