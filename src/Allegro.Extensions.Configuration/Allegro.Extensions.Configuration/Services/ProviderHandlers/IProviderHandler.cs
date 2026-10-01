using Allegro.Extensions.Configuration.Models;

namespace Allegro.Extensions.Configuration.Services.ProviderHandlers;

internal interface IProviderHandler
{
    ConfigurationProviderMetadata GetMetadata();
    string GetRawContent();
}