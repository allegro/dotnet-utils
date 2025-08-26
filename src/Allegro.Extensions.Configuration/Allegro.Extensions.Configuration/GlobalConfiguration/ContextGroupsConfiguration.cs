namespace Allegro.Extensions.Configuration.GlobalConfiguration;

/// <summary>
/// Contains list of the context groups configuration
/// </summary>
public class ContextGroupsConfiguration
{
    /// <summary>
    /// Configuration section name that this class should be bound to
    /// </summary>
    public const string SectionName = "GlobalConfiguration";

    /// <summary>
    /// List of per-context-group configs
    /// </summary>
    public List<ContextGroupConfiguration> ContextGroups { get; init; } = new();
}

/// <summary>
/// Global configuration context group meta-config
/// </summary>
public class ContextGroupConfiguration
{
    /// <summary>
    /// Filesystem path to the files containing the configuration
    /// </summary>
    public string Path { get; init; } = null!;

    /// <summary>
    /// Name of the context group
    /// </summary>
    public string Name { get; init; } = null!;
}