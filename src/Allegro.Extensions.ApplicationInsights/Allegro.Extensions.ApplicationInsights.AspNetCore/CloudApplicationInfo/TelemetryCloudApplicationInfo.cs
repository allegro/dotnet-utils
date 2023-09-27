namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

/// <summary>
/// Information about cloud environment of the app
/// </summary>
public class TelemetryCloudApplicationInfo : Dictionary<string, string>
{
    /// <summary>
    /// Default value when no value provided
    /// </summary>
    public const string EmptyPlaceholder = "empty";

    /// <summary>
    /// ApplicationName Key
    /// </summary>
    public const string ApplicationNameKey = "ApplicationName";

    /// <summary>
    /// Application name (i.e. name of the k8s service)
    /// </summary>
    public string? ApplicationName
    {
        get => this.GetValueOrDefault(nameof(ApplicationName));
        set
        {
            if (value != null)
                this[nameof(ApplicationName)] = value;
        }
    }

    /// <summary>
    /// Load cloud application info from environment variables
    /// </summary>
    public virtual void LoadFromEnv()
    {
        var applicationName =
            Environment.GetEnvironmentVariable(ApplicationNameKey)
            ?? EmptyPlaceholder;

        Add(ApplicationNameKey, applicationName);
    }
}