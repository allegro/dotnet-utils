using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Allegro.Extensions.Configuration.Services.ProviderHandlers;

internal class JsonProviderHandler : GenericProviderHandler<JsonConfigurationProvider>
{
    private static readonly Regex JsonRegex = new(
        @"JsonConfigurationProvider for '(?<filename>.*)' \((.*)\)",
        RegexOptions.Compiled | RegexOptions.ExplicitCapture,
        TimeSpan.FromSeconds(1));

    private readonly string _path;

    public JsonProviderHandler(IConfigurationProvider provider)
        : base(provider)
    {
        var jsonRegexMatch = JsonRegex.Match(Provider.ToString());
        _path = jsonRegexMatch.Success ? jsonRegexMatch.Groups["filename"].Value : Provider.Source.Path;
    }

    public override string GetRawContent()
    {
        using var fileStream = Provider.Source.FileProvider.GetFileInfo(Provider.Source.Path).CreateReadStream();
        using var streamReader = new StreamReader(fileStream);
        return streamReader.ReadToEnd();
    }

    protected override string GetDisplayName()
    {
        return _path;
    }

    protected override string GetKey()
    {
        return _path;
    }

    protected override bool GetIsRawContentAvailable()
    {
        return true;
    }
}