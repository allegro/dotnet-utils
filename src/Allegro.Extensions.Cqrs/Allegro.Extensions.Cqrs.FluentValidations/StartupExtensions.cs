using System.Reflection;
using Allegro.Extensions.Cqrs.Abstractions.Commands;
using Allegro.Extensions.Cqrs.Abstractions.Queries;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Cqrs.FluentValidations;

/// <summary>
/// Startup Extensions - expose registration of Fluent Validations for CQRS in application.
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    /// Add fluent validations for CQRS and register all IValidator from given assemblies
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies">Assembly collection in which IValidators should be looked for.</param>
    /// <param name="publicOnly">Determines whether only public classes should be registered</param>
    public static IServiceCollection AddCqrsFluentValidations(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        bool publicOnly = false)
    {
        services.Scan(s => s.FromAssemblies(assemblies)
            .AddClasses(c => c.AssignableTo(typeof(IValidator<>)), publicOnly)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        var callingAssembly = Assembly.GetCallingAssembly();
        services
            .Scan(s => s.FromAssemblies(callingAssembly)
                .AddClasses(
                    c => c.AssignableToAny(typeof(ICommandValidator<>), typeof(IQueryValidator<>)),
                    publicOnly)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}