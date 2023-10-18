namespace Vabank.Confeature.GlobalConfiguration;

/// <summary>
/// Marks that this global configuration DTO should also be bind with the given configuration section
/// (for example with secrets from KV or local appsettings).
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class MergeWithServiceConfigurationSectionAttribute : Attribute
{
    public string SectionName { get; init; }

    /// <param name="sectionName">Section path to merge with. Null or empty string to merge with root.</param>
    public MergeWithServiceConfigurationSectionAttribute(string sectionName)
    {
        SectionName = sectionName;
    }
}