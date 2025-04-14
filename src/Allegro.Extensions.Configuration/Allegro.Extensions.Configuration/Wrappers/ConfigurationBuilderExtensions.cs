using Microsoft.Extensions.Configuration;

namespace Allegro.Extensions.Configuration.Wrappers
{
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Returns a new <see cref="IConfigurationBuilder"/> that automatically wraps all configuration providers
        /// with a wrapper returned by the <paramref name="wrapperFactory"/>.
        /// </summary>
        /// <param name="builder">Existing <see cref="IConfigurationBuilder"/></param>
        /// <param name="wrapperFactory">Factory of the <see cref="IConfigurationProvider"/> wrapper</param>
        /// <returns>New <see cref="IConfigurationBuilder"/> that will automatically wrap all newly added providers</returns>
        public static IConfigurationBuilder Wrap(
            this IConfigurationBuilder builder,
            Func<IConfigurationProvider, IConfigurationProvider> wrapperFactory)
            => new ConfigurationBuilderWrapper(builder, wrapperFactory);

        /// <summary>
        /// Returns a new <see cref="IConfigurationBuilder"/> that automatically wraps all configuration providers
        /// with a <see cref="SensitiveConfigurationProviderWrapper"/>, which marks the inner provider as sensitive
        /// (meaning that it may contain secret values).
        /// </summary>
        /// <param name="builder">Existing <see cref="IConfigurationBuilder"/></param>
        /// <returns>
        /// New <see cref="IConfigurationBuilder"/> that will automatically wrap all newly added providers
        /// using <see cref="SensitiveConfigurationProviderWrapper"/>.
        /// </returns>
        public static IConfigurationBuilder WrapSensitive(this IConfigurationBuilder builder)
            => builder.Wrap(inner => new SensitiveConfigurationProviderWrapper(inner));

        public static IConfigurationBuilder AddTraversableConfiguration(
            this IConfigurationBuilder builder,
            IConfigurationRoot configurationRoot)
        {
            return builder.Add(new TraversableChainedConfigurationSource(configurationRoot));
        }
    }
}