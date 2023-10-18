using Allegro.Extensions.Configuration.Services;
using Allegro.Extensions.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vabank.Confeature;

namespace Allegro.Extensions.Configuration.FluentValidation;

public static class StartupExtensions
{
    /// <summary>
    /// Registers the configuration using the configuration section name
    /// passed as the sectionName. If sectionName is null, configuration root will be used.
    /// This extension also enables fluent validation for the configuration.
    /// </summary>
    /// <typeparam name="TOptions">Configuration class. It should implement
    /// the <see cref="IConfigurationMarker"/> interface. </typeparam>
    /// <typeparam name="TValidator">Validator class</typeparam>
    public static IServiceCollection RegisterConfig<TOptions, TValidator>(
        this IServiceCollection services,
        IConfiguration configuration,
        string? sectionName = null)
        where TOptions : class, IConfigurationMarker
        where TValidator : class, IValidator<TOptions>
    {
        var configurationSection =
            !string.IsNullOrWhiteSpace(sectionName)
                ? configuration.GetRequiredSection(sectionName)
                : configuration as IConfigurationSection;

        services.AddScoped<IValidator<TOptions>, TValidator>();

        services.Configure<ConfigRegistry>(cr => cr.RegisterOptions<TOptions>(configurationSection?.Path));
        services
            .AddOptions<TOptions>()
            .ValidateDataAnnotations()
            .ValidateFluentValidation()
            .ValidateOnStart()
            .Bind(configurationSection ?? configuration, c => c.BindNonPublicProperties = true);

        return services;
    }
}