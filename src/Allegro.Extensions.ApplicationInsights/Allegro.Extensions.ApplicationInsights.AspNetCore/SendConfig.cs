namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

/// <summary>
/// Configuration for disabling/enabling sending telemetry from local machine
/// </summary>
public record SendConfig
{
    /// <summary>
    /// Disable/enable sending telemetry from local machine
    /// </summary>
    public bool DisableLocally { get; init; } = true;
}