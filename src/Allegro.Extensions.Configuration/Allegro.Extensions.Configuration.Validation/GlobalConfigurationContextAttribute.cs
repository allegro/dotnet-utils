// ReSharper disable ClassNeverInstantiated.Global

namespace Allegro.Extensions.Configuration;

/// <summary>
/// Binds the class marked with the attribute with the configuration context
/// defined by the <see cref="GlobalConfigurationContextAttribute.Context"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class GlobalConfigurationContextAttribute : Attribute
{
    /// <param name="sectionName">Name of the section to be bind with the DTO. Can contain ':' separator.</param>
    public GlobalConfigurationContextAttribute(string sectionName)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
            throw new ArgumentNullException(nameof(sectionName));

        SectionName = sectionName;
    }

    /// <param name="contextGroup">Context group name</param>
    /// <param name="context">Context name</param>
    /// <param name="section">For backward compatibility - ignored</param>
    [Obsolete("For backward compatibility")]
    public GlobalConfigurationContextAttribute(string contextGroup, string context, string section)
        : this(contextGroup, context)
    {
    }

    /// <param name="contextGroup">Context group name</param>
    /// <param name="context">Context name</param>
    [Obsolete(
        "Please use GlobalConfigurationContextAttribute($\"{contextGroup}:{context}\") for backward compatibility. " +
        "For new contexts, pass SectionName defined in context's json files.")]
    public GlobalConfigurationContextAttribute(string contextGroup, string context)
    {
        if (string.IsNullOrWhiteSpace(contextGroup))
            throw new ArgumentNullException(nameof(contextGroup));
        if (string.IsNullOrWhiteSpace(context))
            throw new ArgumentNullException(nameof(context));

#pragma warning disable CS0618
        ContextGroup = contextGroup;
        Context = context;
#pragma warning restore CS0618
        SectionName = $"{contextGroup}:{context}";
    }

    /// <summary>
    /// Global configuration context group name
    /// </summary>
    public string SectionName { get; }

    /// <summary>
    /// Global configuration context group name
    /// </summary>
    [Obsolete($"Please use {nameof(SectionName)} instead")]
    public string? ContextGroup { get; }

    /// <summary>
    /// Global configuration context name
    /// </summary>
    [Obsolete($"Please use {nameof(SectionName)} instead")]
    public string? Context { get; }
}