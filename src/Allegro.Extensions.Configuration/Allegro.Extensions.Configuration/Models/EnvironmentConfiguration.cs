using Vabank.Confeature;

namespace Allegro.Extensions.Configuration.Models;

public class EnvironmentConfiguration : IConfigurationMarker
{
    /// <summary>
    /// Indicates whether service is run in the local, dev, uat or production environment.
    /// </summary>
    public bool IsTestEnvironment { get; set; }
}