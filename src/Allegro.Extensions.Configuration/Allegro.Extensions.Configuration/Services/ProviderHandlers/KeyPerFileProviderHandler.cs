using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.KeyPerFile;

namespace Allegro.Extensions.Configuration.Services.ProviderHandlers;

internal class KeyPerFileProviderHandler : GenericProviderHandler<KeyPerFileConfigurationProvider>
{
    private const string KeyVaultSecretsVolumeMountKey = "KeyVault:SecretsVolumeMount";
    private const string ServiceDiscoverySecretsVolumeMountKey = "ServiceDiscovery:SecretsVolumeMount";

    private static readonly Regex KeyPerFileRegex = new(
        @"KeyPerFileConfigurationProvider for files in '(?<path>.*)' \((?<required>.*)\)",
        RegexOptions.Compiled | RegexOptions.ExplicitCapture,
        TimeSpan.FromSeconds(1));

    private readonly string _displayName = "Key per file";
    private readonly string? _path;

    public KeyPerFileProviderHandler(
        IConfiguration configuration,
        IConfigurationProvider provider)
        : base(provider)
    {
        var keyVaultSecretsVolumeMount = configuration[KeyVaultSecretsVolumeMountKey];
        var serviceDiscoverySecretsVolumeMount = configuration[ServiceDiscoverySecretsVolumeMountKey];

        var keyPerFileRegexMatch = KeyPerFileRegex.Match(Provider.ToString());
        if (!keyPerFileRegexMatch.Success)
        {
            return;
        }

        _path = keyPerFileRegexMatch.Groups["path"].Value;

        if (keyPerFileRegexMatch.Groups["path"].Value.StartsWith(
                keyVaultSecretsVolumeMount,
                StringComparison.InvariantCultureIgnoreCase))
        {
            _displayName = "Key Vault (mount)";
        }
        else if (keyPerFileRegexMatch.Groups["path"].Value.StartsWith(
                     serviceDiscoverySecretsVolumeMount,
                     StringComparison.InvariantCultureIgnoreCase))
        {
            _displayName = "Service Discovery (mount)";
        }
        else
        {
            _displayName = $"Key per file (path: '{keyPerFileRegexMatch.Groups["path"].Value}')";
        }
    }

    protected override string GetDisplayName()
    {
        return _displayName;
    }

    protected override string? GetKey()
    {
        return _path;
    }
}