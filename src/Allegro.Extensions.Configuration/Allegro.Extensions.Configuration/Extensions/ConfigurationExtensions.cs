using System.Reflection;
using Allegro.Extensions.Configuration.Configuration;
using Allegro.Extensions.Configuration.GlobalConfiguration.Provider;
using Microsoft.Extensions.Configuration;
using Vabank.Confeature;

namespace Allegro.Extensions.Configuration.Extensions;

public static class ConfigurationExtensions
{
    /// <summary>
    /// Gets a configuration sub-section with the specified global configuration context.
    /// </summary>
    /// <param name="configuration">Configuration to get the section from</param>
    /// <param name="confeatureOptions">Confeature options</param>
    /// <typeparam name="TGlobalContext">Global configuration context</typeparam>
    /// <returns>The <see cref="Microsoft.Extensions.Configuration.IConfigurationSection" />.</returns>
    /// <remarks>
    ///     If no matching sub-section is found with the specified key or the service is not subscribed to the context,
    ///     an exception is raised.
    /// </remarks>
    public static IConfigurationSection GetGlobalSection<TGlobalContext>(
        this IConfiguration configuration,
        ConfeatureOptions confeatureOptions)
        where TGlobalContext : IGlobalConfigurationMarker
    {
        var attr = typeof(TGlobalContext).GetCustomAttribute<GlobalConfigurationContextAttribute>();
        if (attr is null)
            throw new ArgumentException($"Global configuration DTO {typeof(TGlobalContext)} " +
                                        $"should be marked with the {nameof(GlobalConfigurationContextAttribute)}");

        var serviceName = confeatureOptions.ServiceName;
        var section = configuration.GetSection(attr.SectionName);

        if (!IsServiceSubscribedToGlobalConfiguration(section, serviceName))
        {
            var additionalInfo = string.Empty;

            if (!confeatureOptions.IsEnabled)
            {
                additionalInfo =
                    $"In order to use global contexts, ConfeatureV2 must be enabled. " +
                    $"Please refer to the docs: https://c.qxlint/Confeature-Docs";
            }
            else if (string.IsNullOrEmpty(serviceName))
            {
                additionalInfo =
                    "Looks like you might be running the service locally or executing integration tests. " +
                    "Are you sure the ASPNETCORE_ENVIRONMENT is set to Development?";
            }

            throw new InvalidOperationException(
                $"Service {serviceName} is not subscribed to global configuration " +
                $"section: {attr.SectionName}. " + additionalInfo);
        }

        return section;
    }

    private static bool IsServiceSubscribedToGlobalConfiguration(
        IConfigurationSection section,
        string? serviceName)
    {
        var configServiceName = section.GetValue<string>(ConfeatureConfigurationProvider.ServiceMetadataKeySuffix);
        if (serviceName == configServiceName || configServiceName == ConfeatureConfigurationProvider.AllServicesMarker)
            return true;
        return false;
    }
}