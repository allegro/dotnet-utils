using System.Collections.Concurrent;

namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

/// <summary>
/// Scoped to async local telemetry context which provide additional medata for each telemetry within scope
/// </summary>
public class TelemetryContext : ConcurrentDictionary<string, string>
{
    internal static readonly AsyncLocal<TelemetryContext> _context = new AsyncLocal<TelemetryContext>();

    /// <summary>
    /// Scoped to async local telemetry context which provide additional medata for each telemetry within scope
    /// </summary>
    public static TelemetryContext Current
    {
        get => _context.Value ??= new TelemetryContext();
        private set => _context.Value = value;
    }
}