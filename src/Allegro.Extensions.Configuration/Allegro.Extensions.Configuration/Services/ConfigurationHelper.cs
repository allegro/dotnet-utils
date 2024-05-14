using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using Allegro.Extensions.Configuration.Extensions;
using Allegro.Extensions.Configuration.Models;
using Allegro.Extensions.Configuration.Services.ProviderHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

// ReSharper disable ConvertClosureToMethodGroup

namespace Allegro.Extensions.Configuration.Services;

internal static class ConfigurationHelper
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> ConfigurationTypeValuePropDict = new();

    internal static ConfigurationResponse GetConfiguration(IServiceProvider services)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var configRegistry = services.GetRequiredService<IOptions<ConfigRegistry>>();
        var providersMetadata = GetProvidersMetadata(configuration);
        var keyValues = new Dictionary<string, IList<ValueWithSource>>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var (providerId, (provider, metadata)) in providersMetadata)
        {
            var keys = provider.GetFullKeyNames();
            foreach (var key in keys)
            {
                string? value = null;
                if (!metadata.IsSecret)
                {
                    provider.TryGet(key, out value);
                }

                if (!keyValues.ContainsKey(key))
                {
                    keyValues[key] = new List<ValueWithSource>();
                }

                var configurationClass = configRegistry.Value.TryFindTypeByKey(key);

                keyValues[key].Insert(
                    0,
                    new ValueWithSource(
                        value,
                        providerId,
                        configurationClass?.ConfigurationType.Name,
                        null));
            }
        }

        keyValues = CalculateScheduledValues(services, configRegistry, keyValues, providersMetadata);

        return new ConfigurationResponse(
            keyValues,
            providersMetadata.ToDictionary(
                x => x.Key,
                x => x.Value.Metadata));
    }

    internal static string? GetRawProviderContent(IServiceProvider services, string type, string key)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        if (configuration is not IConfigurationRoot configurationRoot)
        {
            return null;
        }

        foreach (var provider in configurationRoot.Providers)
        {
            var providerHandler = ProviderHandlerFactory.GetProviderHandler(configuration, provider);
            var metadata = providerHandler.GetMetadata();
            if (metadata.Type != type ||
                metadata.Key != key)
            {
                continue;
            }

            if (metadata.IsSecret)
            {
                return null;
            }

            return providerHandler.GetRawContent();
        }

        return null;
    }

    private static Dictionary<string, (IConfigurationProvider Provider, ConfigurationProviderMetadata Metadata)>
        GetProvidersMetadata(IConfiguration configuration)
    {
        var dictionary = new Dictionary<string, (IConfigurationProvider, ConfigurationProviderMetadata)>();

        if (configuration is not IConfigurationRoot configurationRoot)
        {
            return dictionary;
        }

        var providers = configurationRoot.Providers
            .SelectMany(
                provider => provider is ITraversableChainedConfigurationProviderWrapper chained
                    ? chained.ConfigurationRoot.Providers
                    : new[] { provider })
            .ToList();
        for (var i = 0; i < providers.Count; i++)
        {
            dictionary[i.ToString(CultureInfo.InvariantCulture)] = (
                providers[i],
                ProviderHandlerFactory.GetProviderHandler(configuration, providers[i]).GetMetadata());
        }

        return dictionary;
    }

    private static Dictionary<string, IList<ValueWithSource>> CalculateScheduledValues(
        IServiceProvider services,
        IOptions<ConfigRegistry> configRegistry,
        Dictionary<string, IList<ValueWithSource>> keyValues,
        Dictionary<string, (IConfigurationProvider Provider, ConfigurationProviderMetadata Metadata)>
            providersMetadata)
    {
        var logger = new Lazy<ILogger>(
            () => services.GetRequiredService<ILogger<ScheduledConfigurationWrapper<object>>>());
        var scheduledValueInfix = $":{nameof(ScheduledConfigurationWrapper<object>.Schedules)}:";
        var scheduledValuesCandidates = keyValues
            .Where(x => x.Key.Contains(scheduledValueInfix, StringComparison.InvariantCultureIgnoreCase))
            .Where(x => !string.IsNullOrEmpty(x.Value.First().ConfigurationClass))
            .ToList();

        var keyValuePairs = keyValues.ToList();

        foreach (var candidate in scheduledValuesCandidates)
        {
            try
            {
                var keyToWrapper = candidate.Key[..candidate.Key.IndexOf(
                    scheduledValueInfix,
                    StringComparison.InvariantCultureIgnoreCase)];
                var keyToValue = $"{keyToWrapper}:{nameof(ScheduledConfigurationWrapper<object>.Value)}";
                if (keyValues.ContainsKey(keyToValue))
                {
                    keyValuePairs.Remove(candidate);
                    continue;
                }

                var configurationClass = configRegistry.Value.TryFindTypeByKey(keyToWrapper);
                if (configurationClass == null)
                {
                    continue;
                }

                var wrapperPropertyName = keyToWrapper.Split(":").Last();
                var property = configurationClass.ConfigurationType.GetProperty(wrapperPropertyName);
                if (property == null)
                {
                    continue;
                }

                var configurationObj = GetOptionsValue(configurationClass, services);
                var scheduledWrapper = property.GetValue(configurationObj);
                var scheduledWrapperValueProperty = scheduledWrapper?.GetType()
                    .GetProperty(nameof(ScheduledConfigurationWrapper<object>.Value));
                if (scheduledWrapperValueProperty == null)
                {
                    continue;
                }

                var value = scheduledWrapperValueProperty.GetValue(scheduledWrapper);
                var index = keyValuePairs.IndexOf(candidate);

                var originalProviderId = candidate.Value.First().ProviderId;
                var isSecret = providersMetadata.ContainsKey(originalProviderId) &&
                               providersMetadata[originalProviderId].Metadata.IsSecret;

                var valueWithSources = new List<ValueWithSource>
                {
                    new(
                        isSecret || value == null ? null : JsonConvert.SerializeObject(value),
                        originalProviderId,
                        configurationClass.ConfigurationType.Name,
                        "Schedule evaluation"),
                };

                keyValuePairs.Remove(candidate);
                keyValuePairs.RemoveAll(
                    pair => pair.Key ==
                            $"{keyToWrapper}:{nameof(ScheduledConfigurationWrapper<object>.DefaultValue)}");

                keyValuePairs.Insert(
                    index,
                    new KeyValuePair<string, IList<ValueWithSource>>(
                        keyToValue, valueWithSources));
                keyValues.Add(keyToValue, valueWithSources);
            }
            catch (Exception e)
            {
#pragma warning disable CA1848
                logger.Value.LogError(
                    e,
                    "Unable to extract Value of ScheduledConfigurationWrapper for '{Key}'",
                    candidate.Key);
#pragma warning restore CA1848
            }
        }

        return new Dictionary<string, IList<ValueWithSource>>(
            keyValuePairs,
            keyValues.Comparer);
    }

    private static object GetOptionsValue(ConfigRegistration registration, IServiceProvider services)
    {
        var options = services.GetRequiredService(registration.OptionsType);
        var type = options.GetType();
        var valueProp = ConfigurationTypeValuePropDict.GetOrAdd(type, t => t.GetProperty("CurrentValue"));
        return valueProp?.GetValue(options) ?? options;
    }
}