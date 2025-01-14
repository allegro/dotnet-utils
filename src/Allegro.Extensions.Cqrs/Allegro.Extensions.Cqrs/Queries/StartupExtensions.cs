using System.Reflection;
using Allegro.Extensions.Cqrs.Abstractions;
using Allegro.Extensions.Cqrs.Abstractions.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Cqrs.Queries;

/// <summary>
/// Startup Extensions - expose registration of queries in application.
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    /// Register all queries/handlers implemented in application and common tools (dispatchers)
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies">Assembly collection in which Command related types should be looked for.</param>
    /// <param name="publicOnly">Determines whether only public classes should be registered</param>
    public static IServiceCollection AddQueries(this IServiceCollection services, IEnumerable<Assembly> assemblies, bool publicOnly = false)
    {
        services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
        services.Scan(s => s.FromAssemblies(assemblies) // TODO: remove scrutor and register by own util
            .AddClasses(
                c => c.AssignableTo(typeof(IQueryHandler<,>))
                    .WithoutAttribute<DecoratorAttribute>(),
                publicOnly)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services
            .Scan(s => s.FromAssemblies(assemblies)
                .AddClasses(c => c.AssignableTo(typeof(IQueryValidator<>)), publicOnly)
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        return services;
    }
}