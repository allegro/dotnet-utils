using Microsoft.Extensions.Configuration;

namespace Allegro.Extensions.Configuration.GlobalConfiguration.Provider;

/// <summary>
/// Global configuration provider. Can be used to read the global configuration from the files in the path given
/// by the <see cref="ContextGroupsConfiguration"/> or using the fallback service.
/// </summary>
internal class ConfeatureContextConfigurationProvider : ConfigurationProvider
{
    public string ContextName { get; }
    public string ContextGroupName { get; }

    public ConfeatureContextConfigurationProvider(
        IDictionary<string, string> data,
        string contextName,
        string contextGroupName)
    {
        Data = data;
        ContextName = contextName;
        ContextGroupName = contextGroupName;
    }
}