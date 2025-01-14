using System.Reflection;
using Allegro.Extensions.Cqrs.Abstractions;
using Allegro.Extensions.Cqrs.Abstractions.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Cqrs.Commands;

/// <summary>
/// Startup Extensions - expose registration of commands in application.
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    /// Register all commands/handlers implemented in application and common tools (dispatchers, actions, validators)
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies">Assembly collection in which Command related types should be looked for.</param>
    /// <param name="publicOnly">Determines whether only public classes should be registered</param>
    public static IServiceCollection AddCommands(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        bool publicOnly = false)
    {
        services
            .AddSingleton<ICommandDispatcher, CommandDispatcher>();
        services
            .Scan(s => s.FromAssemblies(assemblies) // TODO: should we remove Scrutor in future?
                .AddClasses(
                    c => c.AssignableTo(typeof(ICommandHandler<>))
                        .WithoutAttribute<DecoratorAttribute>(),
                    publicOnly)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        services
            .Scan(s => s.FromAssemblies(assemblies)
                .AddClasses(c => c.AssignableTo(typeof(ICommandValidator<>)), publicOnly)
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        return services;
    }
}