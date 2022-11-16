using System.Collections.Generic;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Validators;

public static class StartupExtensions
{
    /// <summary>
    /// Register all IValidator&lt;T&gt; fluent validators in the assembly
    /// </summary>
    public static IServiceCollection RegisterFluentValidators(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies)
    {
        return services.Scan(s => s.FromAssemblies(assemblies)
            .AddClasses(c => c.AssignableTo(typeof(IValidator<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
    }
}