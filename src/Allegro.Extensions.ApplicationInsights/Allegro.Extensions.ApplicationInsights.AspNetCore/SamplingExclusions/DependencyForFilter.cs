using Microsoft.ApplicationInsights.DataContracts;

namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

/// <summary>
/// Default dependency filter types for ODATA exclusion rules
/// </summary>
[Serializable]
public record DependencyForFilter
{
    /// <summary>
    /// CloudRoleName of dependency
    /// </summary>
    public string CloudRoleName { get; }

    /// <summary>
    /// Type of dependency
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Target of dependency
    /// </summary>
    public string Target { get; }

    /// <summary>
    /// Name of dependency
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Duration of dependency
    /// </summary>
    public double Duration { get; }

    /// <summary>
    /// Success of dependency
    /// </summary>
    public bool? Success { get; }

    /// <summary>
    /// ResultCode of dependency
    /// </summary>
    public string ResultCode { get; }

    /// <summary>
    /// Maps dependencyTelemetry to filter understand by ODATA engine
    /// </summary>
    /// <param name="dependencyTelemetry"></param>
    public DependencyForFilter(DependencyTelemetry dependencyTelemetry)
    {
        Type = dependencyTelemetry.Type ?? string.Empty;
        Target = dependencyTelemetry.Target ?? string.Empty;
        Name = dependencyTelemetry.Name ?? string.Empty;
        Duration = dependencyTelemetry.Duration.TotalMilliseconds;
        Success = dependencyTelemetry.Success;
        CloudRoleName = dependencyTelemetry.Context?.Cloud?.RoleName ?? string.Empty;
        ResultCode = dependencyTelemetry.ResultCode ?? string.Empty;
    }
}