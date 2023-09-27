using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

internal class TelemetryContextInitializer : ITelemetryInitializer
{
    private readonly IHttpContextAccessor _contextAccessor;

    public TelemetryContextInitializer(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public void Initialize(ITelemetry telemetry)
    {
        TelemetryContext context;

        if (TelemetryContext._context?.Value == null)
        {
            if (_contextAccessor.HttpContext?.Items != null && _contextAccessor.HttpContext.Items.ContainsKey(nameof(TelemetryContext)))
            {
                context = (TelemetryContext)_contextAccessor.HttpContext.Items[nameof(TelemetryContext)];
            }
            else
            {
                return;
            }
        }
        else
        {
            context = TelemetryContext.Current;
        }

        var propTelemetry = (ISupportProperties)telemetry;

        foreach (var keyValue in context)
            SetProperty(propTelemetry, keyValue.Key, keyValue.Value);
    }

    private static void SetProperty(ISupportProperties propTelemetry, string key, string value)
    {
        if (!string.IsNullOrEmpty(value) &&
            !propTelemetry.Properties.ContainsKey(key))
            propTelemetry.Properties.Add(key, value);
    }
}