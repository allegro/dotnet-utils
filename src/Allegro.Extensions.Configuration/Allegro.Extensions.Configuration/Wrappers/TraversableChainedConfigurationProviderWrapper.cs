using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Allegro.Extensions.Configuration.Wrappers
{
    /// <summary>
    /// <see cref="IConfigurationProvider"/> wrapper on <see cref="ChainedConfigurationProvider"/> that
    /// exposes the IConfigurationRoot being wrapped.
    /// </summary>
    public class TraversableChainedConfigurationProviderWrapper : ITraversableChainedConfigurationProviderWrapper
    {
        public IConfigurationRoot ConfigurationRoot { get; }

        private readonly IConfigurationProvider _inner;

        public TraversableChainedConfigurationProviderWrapper(IConfigurationRoot configurationRoot)
        {
            ConfigurationRoot = configurationRoot;
            _inner = new ChainedConfigurationProvider(
                new ChainedConfigurationSource
                {
                    Configuration = configurationRoot
                });
        }

        public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
            => _inner.GetChildKeys(earlierKeys, parentPath);

        public IChangeToken GetReloadToken()
            => _inner.GetReloadToken();

        public void Load()
            => _inner.Load();

        public void Set(string key, string value)
            => _inner.Set(key, value);

        public bool TryGet(string key, out string value)
            => _inner.TryGet(key, out value);
    }
}