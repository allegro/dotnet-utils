using Microsoft.Extensions.Configuration;

// ReSharper disable ConvertClosureToMethodGroup

namespace Allegro.Extensions.Configuration.Extensions;

internal static class ConfigurationProviderExtensions
{
    internal static HashSet<string> GetFullKeyNames(
        this IConfigurationProvider provider,
        string? keyPrefix = null,
        HashSet<string>? initialKeys = null)
    {
        initialKeys ??= new HashSet<string>();
        foreach (var key in provider.GetChildKeys(Enumerable.Empty<string>(), keyPrefix).Distinct())
        {
            var childKeyPrefix = string.IsNullOrWhiteSpace(keyPrefix) ? key : $"{keyPrefix}:{key}";

            GetFullKeyNames(provider, childKeyPrefix, initialKeys);

            if (!initialKeys.Any(k => k.StartsWith(childKeyPrefix, StringComparison.OrdinalIgnoreCase)))
            {
                initialKeys.Add(childKeyPrefix);
            }
        }

        return initialKeys;
    }

    internal static IConfigurationProvider GetInnermostProvider(this IConfigurationProvider provider)
    {
        while (provider is IConfigurationProviderWrapper wrapper)
        {
            provider = wrapper.Inner;
        }

        return provider;
    }
}