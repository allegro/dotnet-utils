using Microsoft.ApplicationInsights.DataContracts;

namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

/// <summary>
/// Default request filter types for ODATA exclusion rules
/// </summary>
[Serializable]
public record RequestForFilter
{
    /// <summary>
    /// Maps requestTelemetry to filter understand by ODATA engine
    /// </summary>
    /// <param name="requestTelemetry"></param>
    public RequestForFilter(RequestTelemetry requestTelemetry)
    {
        Name = requestTelemetry.Name ?? string.Empty;
        Duration = requestTelemetry.Duration.TotalMilliseconds;
        Success = requestTelemetry.Success;
        CloudRoleName = requestTelemetry.Context?.Cloud?.RoleName ?? string.Empty;
        ResponseCode = requestTelemetry.ResponseCode ?? string.Empty;
        Url = requestTelemetry.Url?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// CloudRoleName of request
    /// </summary>
    public string CloudRoleName { get; }

    /// <summary>
    /// Name of request
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Duration of request
    /// </summary>
    public double Duration { get; }

    /// <summary>
    /// Success of request
    /// </summary>
    public bool? Success { get; }

    /// <summary>
    /// ResponseCode of request
    /// </summary>
    public string ResponseCode { get; }

    /// <summary>
    /// Url of request
    /// </summary>
    public string Url { get; }
}