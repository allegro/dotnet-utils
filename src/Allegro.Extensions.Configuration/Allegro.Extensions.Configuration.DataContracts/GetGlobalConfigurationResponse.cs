namespace Allegro.Extensions.Configuration.DataContracts;

/// <summary>
/// Global configuration response
/// </summary>
public class GetGlobalConfigurationResponse
{
    /// <summary>
    /// List of context groups
    /// </summary>
    public List<ContextGroupModel> ContextGroups { get; init; } = new();
}

/// <summary>
/// Context group representation
/// </summary>
public class ContextGroupModel
{
    /// <summary>
    /// Name of the Context Group
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Contexts defined within the context group
    /// </summary>
    public List<string> Contexts { get; init; } = new();
}