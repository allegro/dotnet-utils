using Allegro.Extensions.Configuration.DataContracts;
using Allegro.Extensions.Configuration.Extensions;
using Allegro.Extensions.Configuration.GlobalConfiguration;
using Microsoft.Extensions.Options;

namespace Allegro.Extensions.Configuration.Api.Services;

public interface IGlobalConfigurationProvider
{
    GetGlobalConfigurationResponse GetGlobalConfiguration(string? serviceName = null);
    Stream GetContext(string contextGroupName, string contextName);
}

public class GlobalConfigurationProvider : IGlobalConfigurationProvider
{
    private readonly ContextGroupsConfiguration _configuration;

    public GlobalConfigurationProvider(IOptions<ContextGroupsConfiguration> configuration)
    {
        _configuration = configuration.Value;
    }

    public GetGlobalConfigurationResponse GetGlobalConfiguration(string? serviceName = null)
    {
#pragma warning disable CSE001
        var result = new GetGlobalConfigurationResponse();
#pragma warning restore CSE001

        foreach (var contextGroupConfiguration in _configuration.ContextGroups)
        {
            var contexts = GetContextsList(contextGroupConfiguration, serviceName).OrderBy(c => c).ToList();
            if (contexts.Count == 0)
            {
                continue;
            }

            result.ContextGroups.Add(
                new ContextGroupModel { Name = contextGroupConfiguration.Name, Contexts = contexts });
        }

        return result;
    }

    public Stream GetContext(string contextGroupName, string contextName)
    {
        var contextGroup = _configuration.ContextGroups
            .Single(x => x.Name.Equals(contextGroupName, StringComparison.OrdinalIgnoreCase));
        return File.OpenRead(Path.Combine(contextGroup.Path, $"{contextName}.json"));
    }

    private static IEnumerable<string> GetContextsList(
        ContextGroupConfiguration contextGroupConfiguration,
        string? serviceName)
    {
        foreach (var file in Directory.EnumerateFiles(contextGroupConfiguration.Path, "*.json"))
        {
            if (!string.IsNullOrEmpty(serviceName) &&
                !ConfigurationContextExtensions.IsServiceListedForContext(File.OpenRead(file), serviceName))
            {
                continue;
            }

            yield return Path.GetFileNameWithoutExtension(file);
        }
    }
}