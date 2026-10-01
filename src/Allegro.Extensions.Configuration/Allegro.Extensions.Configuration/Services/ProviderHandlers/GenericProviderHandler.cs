using Allegro.Extensions.Configuration.Exceptions;
using Allegro.Extensions.Configuration.Models;
using Microsoft.Extensions.Configuration;

namespace Allegro.Extensions.Configuration.Services.ProviderHandlers;

internal class GenericProviderHandler<TProvider> : IProviderHandler
    where TProvider : IConfigurationProvider
{
    private readonly string? _displayName;
    protected readonly TProvider Provider;
    protected readonly bool IsSecret;

    public GenericProviderHandler(IConfigurationProvider provider)
    {
        IsSecret = provider is ISensitiveConfigurationProviderWrapper;
        while (provider is IConfigurationProviderWrapper providerWrapper)
        {
            provider = providerWrapper.Inner;
            IsSecret = IsSecret || providerWrapper is ISensitiveConfigurationProviderWrapper;
        }

        if (provider is not TProvider castedProvider)
        {
            throw new InvalidProviderTypeException(provider.GetType());
        }

        Provider = castedProvider;
    }

    public GenericProviderHandler(IConfigurationProvider provider, string displayName)
        : this(provider)
    {
        _displayName = displayName;
    }

    public ConfigurationProviderMetadata GetMetadata()
    {
        return new ConfigurationProviderMetadata(
            GetDisplayName(),
            typeof(TProvider).Name,
            GetKey(),
            IsSecret,
            GetIsRawContentAvailable());
    }

    public virtual string GetRawContent() => throw new NotSupportedException();

    protected virtual string GetDisplayName()
        => _displayName ?? Provider!.ToString() ?? Provider!.GetType().Name;

    protected virtual string? GetKey() => null;

    protected virtual bool GetIsRawContentAvailable() => false;
}