using Microsoft.Extensions.Logging;

namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

/// <summary>
/// Separate instance of logger for logging purposes in TelemetryInitializers
/// </summary>
public interface ITelemetryInitializerLogger : ILogger
{ }

internal class TelemetryInitializerLogger : Logger<TelemetryInitializerLogger>, ITelemetryInitializerLogger
{
    public TelemetryInitializerLogger(ILoggerFactory factory) : base(factory)
    {
    }
}