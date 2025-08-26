using System.Reflection;
using Microsoft.Extensions.Options;

namespace Allegro.Extensions.Configuration.Services;

/// <summary>
/// Holds information about registered configuration class
/// </summary>
/// <param name="ConfigurationType">Type with configuration</param>
/// <param name="OptionsType">IOptionsMonitor wrapper for the configuration class</param>
/// <param name="Prefix">Name of the configuration section used to register the configuration or configuration key</param>
internal record ConfigRegistration(
    Type ConfigurationType,
    Type OptionsType,
    string Prefix);

/// <summary>
/// Holds information about all registered configuration types
/// </summary>
internal class ConfigRegistry
{
    private readonly List<ConfigRegistration> _registrations = new();

    /// <summary>
    /// All registered configurations wrapped in IOptionsSnapshot
    /// </summary>
    internal IEnumerable<Type> OptionsTypes =>
        _registrations.Select(t => t.OptionsType);

    /// <summary>
    /// Registers configuration type
    /// </summary>
    /// <param name="sectionName">Name of the configuration section used to register the configuration (or null for root)</param>
    /// <typeparam name="TConfiguration">Type with configuration</typeparam>
    internal void RegisterOptions<TConfiguration>(string? sectionName = null)
    {
        var configurationType = typeof(TConfiguration);

        if (sectionName != null)
        {
            _registrations.Add(
                new ConfigRegistration(
                    configurationType,
                    typeof(IOptionsMonitor<TConfiguration>),
                    sectionName));
        }
        else
        {
            // when binding to root, we need to register all properties individually
            foreach (var property in configurationType.GetProperties(
                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                _registrations.Add(
                    new ConfigRegistration(
                        configurationType,
                        typeof(IOptionsMonitor<TConfiguration>),
                        property.Name));
            }
        }
    }

    /// <summary>
    /// Searches for the configuration type registered using specified configuration key
    /// </summary>
    /// <param name="key">The key used to register the configuration type</param>
    /// <returns>Configuration registration info</returns>
    internal ConfigRegistration? TryFindTypeByKey(string key)
        => _registrations.FirstOrDefault(
            x => key.StartsWith($"{x.Prefix}:", StringComparison.InvariantCultureIgnoreCase) ||
                 key.Equals(x.Prefix, StringComparison.OrdinalIgnoreCase));
}