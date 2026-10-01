using Microsoft.Extensions.Configuration;

namespace Allegro.Extensions.Configuration.Wrappers
{
    /// <summary>
    /// <see cref="IConfigurationProvider"/> wrapper that marks the inner provider as sensitive
    /// (meaning that it may contain secret values).
    /// </summary>
    public class SensitiveConfigurationProviderWrapper :
        ConfigurationProviderWrapperBase,
        ISensitiveConfigurationProviderWrapper
    {
        public SensitiveConfigurationProviderWrapper(IConfigurationProvider inner) : base(inner)
        {
        }

        public override string? ToString()
        {
            return $"{Inner} (sensitive)";
        }
    }
}