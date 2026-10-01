using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Allegro.Extensions.Configuration.Wrappers
{
    /// <summary>
    /// Base class for <see cref="IConfigurationProvider"/> wrappers, that simplifies traversing the providers' graph
    /// by exposing the <see cref="Inner"/> property.
    /// </summary>
    public abstract class ConfigurationProviderWrapperBase : IConfigurationProvider, IConfigurationProviderWrapper
    {
        /// <summary>
        /// Inner <see cref="IConfigurationProvider"/> being wrapped.
        /// </summary>
        public IConfigurationProvider Inner { get; }

        protected ConfigurationProviderWrapperBase(IConfigurationProvider inner)
        {
            Inner = inner;
        }

        public override string? ToString()
        {
            return Inner.ToString();
        }

        public virtual IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
            => Inner.GetChildKeys(earlierKeys, parentPath);

        public virtual IChangeToken GetReloadToken()
            => Inner.GetReloadToken();

        public virtual void Load()
            => Inner.Load();

        public virtual void Set(string key, string value)
            => Inner.Set(key, value);

        public virtual bool TryGet(string key, out string value)
            => Inner.TryGet(key, out value);
    }
}