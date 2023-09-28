using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Hosting;

namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

internal class FlushAppInsightsHostedService : IHostedService
{
    private readonly TelemetryClient _telemetryClient;
    private readonly IHostApplicationLifetime _hostLifetime;

    public FlushAppInsightsHostedService(TelemetryClient telemetryClient, IHostApplicationLifetime hostLifetime)
    {
        _telemetryClient = telemetryClient;
        _hostLifetime = hostLifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // flushes right after graceful shutdown of hosted services finished
        _hostLifetime.ApplicationStopped.Register(() => _telemetryClient.Flush());

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // flushes right after graceful shutdown of hosted services started
        _telemetryClient.Flush();

        return Task.CompletedTask;
    }
}