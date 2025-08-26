using Microsoft.Extensions.Configuration;

namespace Allegro.Extensions.Configuration
{
    /// <summary>
    /// <see cref="Microsoft.Extensions.Configuration.IConfigurationProvider"/> wrapper, that simplifies traversing the providers' graph
    /// by exposing the <see cref="Inner"/> property.
    /// </summary>
    public interface IConfigurationProviderWrapper : IConfigurationProvider
    {
        /// <summary>
        /// Inner <see cref="IConfigurationProvider"/> being wrapped.
        /// </summary>
        IConfigurationProvider Inner { get; }
    }
}