using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;

namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

internal class TelemetryCloudApplicationInfoTelemetryInitializer : ITelemetryInitializer
{
    private readonly IOptions<TelemetryCloudApplicationInfo> _cloudApplicationInfo;

    public TelemetryCloudApplicationInfoTelemetryInitializer(IOptions<TelemetryCloudApplicationInfo> cloudApplicationInfo)
    {
        _cloudApplicationInfo = cloudApplicationInfo;
    }

    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = _cloudApplicationInfo.Value.ApplicationName;

        foreach (var metadata in _cloudApplicationInfo.Value)
        {
            if (metadata.Key != TelemetryCloudApplicationInfo.ApplicationNameKey)
            {
                SetTelemetryProperty(telemetry, metadata.Key, metadata.Value);
            }
        }
    }

    private static void SetTelemetryProperty(ITelemetry telemetry, string key, string value)
    {
        if (telemetry is ISupportProperties propTelemetry &&
            !string.IsNullOrEmpty(value) &&
            !propTelemetry.Properties.ContainsKey(key))
        {
            propTelemetry.Properties.Add(key, value);
        }
    }
}