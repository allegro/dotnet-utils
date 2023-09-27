using Allegro.Extensions.ApplicationInsights.AspNetCore;

namespace Allegro.Extensions.ApplicationInsights.Demo;

internal class CustomTelemetryCloudApplicationInfo : TelemetryCloudApplicationInfo
{
    /// <summary>
    /// Name of the team assigned to the service
    /// </summary>
    public string? TeamName
    {
        get => this.GetValueOrDefault(nameof(TeamName));
        set
        {
            if (value != null)
            {
                this[nameof(TeamName)] = value;
            }
        }
    }

    public override void LoadFromEnv()
    {
        base.LoadFromEnv();
        Add(nameof(TeamName), Environment.GetEnvironmentVariable(nameof(TeamName)) ?? EmptyPlaceholder);
    }
}