using Microsoft.Extensions.Configuration;

namespace Allegro.Extensions.Configuration.Wrappers
{
    /// <summary>
    /// Wrapper for <see cref="IConfigurationSource"/> that automatically wraps the <see cref="IConfigurationProvider"/>
    /// using provided <see cref="WrapperFactory"/>.
    /// </summary>
    public class ConfigurationSourceWrapper : IConfigurationSource
    {
        /// <summary>
        /// Inner <see cref="IConfigurationSource"/> being wrapped.
        /// </summary>
        public IConfigurationSource Inner { get; }

        /// <summary>
        /// Factory of the <see cref="IConfigurationProvider"/> wrapper.
        /// </summary>
        public Func<IConfigurationProvider, IConfigurationProvider> WrapperFactory { get; }

        public ConfigurationSourceWrapper(
            IConfigurationSource inner,
            Func<IConfigurationProvider, IConfigurationProvider> wrapperFactory)
        {
            Inner = inner;
            WrapperFactory = wrapperFactory;
        }

        public override string? ToString() => Inner.ToString();

        public IConfigurationProvider Build(IConfigurationBuilder builder) => WrapperFactory(Inner.Build(builder));
    }
}