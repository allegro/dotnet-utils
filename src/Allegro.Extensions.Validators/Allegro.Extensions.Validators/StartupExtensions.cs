using System.Collections.Generic;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegro.Extensions.Validators;

/// <summary>
/// Startup extensions for FluentValidation
/// </summary>
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

    /// <summary>
    /// Adds a new IOptions&lt;TOptions&gt; to the service collection.
    /// </summary>
    public static OptionsBuilder<TOptions> AddWithFluentValidation<TOptions, TValidator>(
        this IServiceCollection services,
        string? configurationSection)
        where TOptions : class
        where TValidator : class, IValidator<TOptions>
    {
        // Add the validator
        services.AddSingleton<IValidator<TOptions>, TValidator>();

        return services.AddOptions<TOptions>()
            .BindConfiguration(configurationSection)
            .ValidateFluentValidation()
            .ValidateOnStart();
    }
}