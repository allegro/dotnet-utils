using System.Reflection;
using Allegro.Extensions.Cqrs.Abstractions.Commands;
using Allegro.Extensions.Cqrs.Abstractions.Queries;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Cqrs.FluentValidations;

/// <summary>
/// 
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IServiceCollection AddCqrsFluentValidations(this IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        services.Scan(s => s.FromAssemblies(assemblies)
            .AddClasses(c => c.AssignableTo(typeof(IValidator<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services
            .Scan(s => s.FromCallingAssembly()
                .AddClasses(c => c.AssignableToAny(typeof(ICommandValidator<>), typeof(IQueryValidator<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}