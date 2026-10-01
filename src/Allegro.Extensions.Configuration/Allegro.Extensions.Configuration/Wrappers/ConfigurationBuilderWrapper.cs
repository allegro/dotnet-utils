using Microsoft.Extensions.Configuration;

namespace Allegro.Extensions.Configuration.Wrappers
{
    /// <summary>
    /// Wrapper for <see cref="IConfigurationBuilder"/> that automatically wraps all newly added configuration providers
    /// using provided <see cref="WrapperFactory"/>.
    /// </summary>
    public class ConfigurationBuilderWrapper : IConfigurationBuilder
    {
        /// <summary>
        /// Inner <see cref="IConfigurationBuilder"/> being wrapped.
        /// </summary>
        public IConfigurationBuilder Inner { get; }

        /// <summary>
        /// Factory of the <see cref="IConfigurationProvider"/> wrapper.
        /// </summary>
        public Func<IConfigurationProvider, IConfigurationProvider> WrapperFactory { get; }

        public ConfigurationBuilderWrapper(
            IConfigurationBuilder inner,
            Func<IConfigurationProvider, IConfigurationProvider> wrapperFactory)
        {
            Inner = inner;
            WrapperFactory = wrapperFactory;
        }

        public override string? ToString() => Inner.ToString();

        public IConfigurationBuilder Add(IConfigurationSource source) =>
            Inner.Add(new ConfigurationSourceWrapper(source, WrapperFactory));

        public IConfigurationRoot Build() => Inner.Build();

        public IDictionary<string, object> Properties => Inner.Properties;

        public IList<IConfigurationSource> Sources => Inner.Sources;
    }
}