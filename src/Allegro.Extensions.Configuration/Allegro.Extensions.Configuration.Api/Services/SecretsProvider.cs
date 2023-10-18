using Allegro.Extensions.Configuration.Api.Configuration;
using Allegro.Extensions.Configuration.Api.Exceptions;
using Allegro.Extensions.Configuration.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Allegro.Extensions.Configuration.Api.Services;

public interface ISecretsProvider
{
    string GetSecretsAsJson(IEnumerable<string> keyVaultPrefixes);
}

public class SecretsProvider : ISecretsProvider
{
    private readonly EnvironmentConfiguration _environmentConfiguration;
    private readonly SecretsConfiguration _secretsConfiguration;

    public SecretsProvider(
        IOptions<EnvironmentConfiguration> environmentConfiguration,
        IOptions<SecretsConfiguration> secretsConfiguration)
    {
        _environmentConfiguration = environmentConfiguration.Value;
        _secretsConfiguration = secretsConfiguration.Value;
    }

    public string GetSecretsAsJson(IEnumerable<string> keyVaultPrefixes)
    {
        if (!_environmentConfiguration.IsTestEnvironment)
        {
            throw new NotAllowedOutsideTestEnvException();
        }

        var secretsConfiguration =
            new ConfigurationBuilder()
                .AddKeyPerFile(_secretsConfiguration.SecretsPath)
                .Build();

        var prefixes = NormalizeKeyVaultPrefixes(keyVaultPrefixes);
        return JsonConvert.SerializeObject(
            secretsConfiguration
                .AsEnumerable()
                .Where(x => prefixes.Any(p => x.Key.StartsWith(p, StringComparison.InvariantCultureIgnoreCase)))
                .ToDictionary(x => x.Key, x => x.Value));
    }

    private static string[] NormalizeKeyVaultPrefixes(IEnumerable<string> prefixes)
        => prefixes
            .Select(p => p.Replace("--", ConfigurationPath.KeyDelimiter))
            .ToArray();
}