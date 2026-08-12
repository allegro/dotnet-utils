using System.Reflection;
using Allegro.Extensions.Configuration.Configuration;
using Allegro.Extensions.Configuration.GlobalConfiguration;
using Allegro.Extensions.Configuration.Models;
using Allegro.Extensions.Configuration.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ConvertClosureToMethodGroup

namespace Allegro.Extensions.Configuration.Extensions;

public static class StartupExtensions
{
    /// <summary>
    /// Registers Confeature V2 dependencies.
    /// </summary>
    public static IServiceCollection AddConfeature(
        this IServiceCollection services,
        ConfeatureOptions confeatureOptions)
    {
        services.AddTransient<IStartupFilter, ConfeatureStartupFilter>();
        services.AddSingleton<IConfigurationPrinter, ConfigurationPrinter>();
        services.AddSingleton(confeatureOptions);
        services.AddOptions<ContextGroupsConfiguration>().BindConfiguration(ContextGroupsConfiguration.SectionName);
        services.AddOptions<EnvironmentConfiguration>(); // TODO set in platform
        return services;
    }

    /// <summary>
    /// Registers the configuration using the configuration section name
    /// passed as the sectionName. If sectionName is null, configuration root will be used.
    /// </summary>
    /// <typeparam name="T">Configuration class. It should implement
    /// the <see cref="IConfigurationMarker"/> interface. </typeparam>
    public static IServiceCollection RegisterConfig<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        string? sectionName = null)
        where T : class, IConfigurationMarker
    {
        var configurationSection =
            !string.IsNullOrWhiteSpace(sectionName)
                ? configuration.GetRequiredSection(sectionName)
                : configuration as IConfigurationSection;

        services.Configure<ConfigRegistry>(cr => cr.RegisterOptions<T>(configurationSection?.Path));
        services
            .AddOptions<T>()
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .Bind(configurationSection ?? configuration, c => c.BindNonPublicProperties = true);

        return services;
    }

    /// <summary>
    /// Registers the global configuration.
    /// </summary>
    /// <typeparam name="T">Type that implements the <see cref="IGlobalConfigurationMarker"/> interface
    /// and has the <see cref="GlobalConfigurationContextAttribute"/> attribute.</typeparam>
    /// <exception cref="ArgumentException">Thrown when no <see cref="GlobalConfigurationContextAttribute"/> is found</exception>
    /// <exception cref="ArgumentNullException">Thrown when sectionName from the
    /// <see cref="GlobalConfigurationContextAttribute"/> is null</exception>
    public static IServiceCollection RegisterGlobalConfig<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        ConfeatureOptions confeatureOptions)
        where T : class, IGlobalConfigurationMarker
    {
        var section = configuration.GetGlobalSection<T>(confeatureOptions);

        var mergeAttrs = typeof(T).GetCustomAttributes<MergeWithServiceConfigurationSectionAttribute>();
        foreach (var mergeAttr in mergeAttrs)
        {
            services.RegisterConfig<T>(configuration, mergeAttr.SectionName);
        }

        return services.RegisterConfig<T>(section);
    }
}