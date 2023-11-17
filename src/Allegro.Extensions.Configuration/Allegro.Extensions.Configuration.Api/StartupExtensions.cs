using Allegro.Extensions.Configuration.Api.Services;
using Allegro.Extensions.Configuration.Configuration;
using Allegro.Extensions.Configuration.Extensions;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ConvertClosureToMethodGroup

namespace Allegro.Extensions.Configuration.Api;

public static class StartupExtensions
{
    /// <summary>
    /// Registers Confeature V2 dependencies for a Fallback Service.
    /// </summary>
    public static IServiceCollection AddConfeatureFallbackService(
        this IServiceCollection services,
        ConfeatureOptions confeatureOptions)
    {
        services.AddConfeature(confeatureOptions);
        services.AddSingleton<IGlobalConfigurationProvider, GlobalConfigurationProvider>();
        services.AddSingleton<ISecretsProvider, SecretsProvider>();
        return services;
    }
}