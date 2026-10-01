using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Allegro.Extensions.Configuration.Wrappers
{
    public static class KeyPerFileConfigurationExtensions
    {
        public static IConfigurationBuilder AddKeyPerFileFiltered(
            this IConfigurationBuilder configurationBuilder,
            string path,
            params string[] allowedKeyPrefixes)
        {
            return configurationBuilder.AddKeyPerFile(source =>
            {
                source.FileProvider = new PhysicalFileProvider(path);
                source.Optional = false;
                source.IgnoreCondition = name =>
                    allowedKeyPrefixes.All(prefix =>
                        !name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
            });
        }
    }
}