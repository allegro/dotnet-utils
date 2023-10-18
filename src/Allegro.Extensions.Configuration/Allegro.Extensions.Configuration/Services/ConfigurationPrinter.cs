using Allegro.Extensions.Configuration.Models;

namespace Allegro.Extensions.Configuration.Services;

/// <summary>
/// Defines methods for retrieving the configuration ready to be pretty-printed on the UI
/// </summary>
public interface IConfigurationPrinter
{
    /// <summary>
    /// Returns all registered configurations from all providers, including scheduled values
    /// and KeyVault secrets (secrets' values are not shown).
    /// </summary>
    ConfigurationResponse GetConfiguration();

    /// <summary>
    /// Returns string containing the raw view of the provider (e.g. full JSON file for the JsonConfigurationProvider)
    /// </summary>
    string? GetRawProviderContent(string type, string key);
}

/// <inheritdoc cref="IConfigurationPrinter"/>
public sealed class ConfigurationPrinter : IConfigurationPrinter
{
    private readonly IServiceProvider _services;

    /// <param name="services">Service provider for retrieving the registered configuration objects</param>
    public ConfigurationPrinter(IServiceProvider services) => _services = services;

    /// <inheritdoc cref="IConfigurationPrinter.GetConfiguration"/>
    public ConfigurationResponse GetConfiguration()
    {
        return ConfigurationHelper.GetConfiguration(_services);
    }

    /// <inheritdoc cref="IConfigurationPrinter.GetRawProviderContent"/>
    public string? GetRawProviderContent(string type, string key)
    {
        return ConfigurationHelper.GetRawProviderContent(_services, type, key);
    }
}