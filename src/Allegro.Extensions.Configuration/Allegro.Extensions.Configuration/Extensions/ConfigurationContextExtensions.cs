using System.Text.Json;

namespace Allegro.Extensions.Configuration.Extensions;

public static class ConfigurationContextExtensions
{
    public static bool IsServiceListedForContext(Stream jsonStream, string serviceName)
    {
        using var document = JsonDocument.Parse(jsonStream);
        if (!document.RootElement.TryGetProperty("metadata", out var metadataProperty))
        {
            return false;
        }

        if (!metadataProperty.TryGetProperty("services", out var servicesProperty))
        {
            return false;
        }

        return servicesProperty.EnumerateArray()
            .Any(x => x.GetString() == serviceName || x.GetString() == "*");
    }
}