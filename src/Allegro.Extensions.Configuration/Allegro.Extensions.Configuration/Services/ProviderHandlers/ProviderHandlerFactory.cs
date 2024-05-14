using Allegro.Extensions.Configuration.Extensions;
using Allegro.Extensions.Configuration.GlobalConfiguration.Provider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.KeyPerFile;

namespace Allegro.Extensions.Configuration.Services.ProviderHandlers;

internal static class ProviderHandlerFactory
{
    internal static IProviderHandler GetProviderHandler(
        IConfiguration configuration,
        IConfigurationProvider provider)
    {
        return provider.GetInnermostProvider() switch
        {
            ConfeatureContextConfigurationProvider => new ConfeatureContextProviderHandler(provider),
            JsonConfigurationProvider => new JsonProviderHandler(provider),
            ChainedConfigurationProvider =>
                new GenericProviderHandler<ChainedConfigurationProvider>(
                    provider,
                    "Chained"),
            EnvironmentVariablesConfigurationProvider =>
                new GenericProviderHandler<EnvironmentVariablesConfigurationProvider>(
                    provider,
                    "Env vars"),
            KeyPerFileConfigurationProvider =>
                new KeyPerFileProviderHandler(
                    configuration,
                    provider),
            _ => new GenericProviderHandler<IConfigurationProvider>(provider),
        };
    }
}