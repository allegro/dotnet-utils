using Allegro.Extensions.Configuration.GlobalConfiguration.Provider;
using Microsoft.Extensions.Configuration;

namespace Allegro.Extensions.Configuration.Services.ProviderHandlers;

internal class ConfeatureContextProviderHandler : GenericProviderHandler<ConfeatureContextConfigurationProvider>
{
    public ConfeatureContextProviderHandler(IConfigurationProvider provider)
        : base(provider)
    {
    }

    public override string GetRawContent() => throw new NotSupportedException();

    protected override string GetDisplayName() => $"Global ({Provider.ContextName})";

    protected override string? GetKey() => $"{Provider.ContextGroupName};{Provider.ContextName}";

    protected override bool GetIsRawContentAvailable() => false;
}