using Microsoft.Extensions.Configuration;

namespace Allegro.Extensions.Configuration
{
    /// <summary>
    /// <see cref="IConfigurationRoot"/> wrapper, that simplifies traversing the providers' graph
    /// by exposing the <see cref="ConfigurationRoot"/> property.
    /// </summary>
    public interface ITraversableChainedConfigurationProviderWrapper : IConfigurationProvider
    {
        /// <summary>
        /// Inner <see cref="IConfigurationRoot"/> being wrapped.
        /// </summary>
        IConfigurationRoot ConfigurationRoot { get; }
    }
}