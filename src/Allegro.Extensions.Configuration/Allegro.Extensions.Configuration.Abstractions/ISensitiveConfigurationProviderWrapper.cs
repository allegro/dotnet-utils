using Microsoft.Extensions.Configuration;

namespace Allegro.Extensions.Configuration
{
    /// <summary>
    /// <see cref="IConfigurationProvider"/> wrapper that marks the inner provider as sensitive
    /// (meaning that it may contain secret values).
    /// </summary>
    public interface ISensitiveConfigurationProviderWrapper : IConfigurationProviderWrapper
    {
    }
}