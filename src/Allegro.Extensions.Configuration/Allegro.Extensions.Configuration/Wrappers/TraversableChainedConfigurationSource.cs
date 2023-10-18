using Microsoft.Extensions.Configuration;

namespace Allegro.Extensions.Configuration.Wrappers
{
    /// <summary>
    /// <see cref="IConfigurationSource"/> for <see cref="TraversableChainedConfigurationProviderWrapper"/>.
    /// </summary>
    public class TraversableChainedConfigurationSource : IConfigurationSource
    {
        private readonly IConfigurationRoot _configurationRoot;

        public TraversableChainedConfigurationSource(IConfigurationRoot configurationRoot)
        {
            _configurationRoot = configurationRoot;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new TraversableChainedConfigurationProviderWrapper(_configurationRoot);
        }
    }
}