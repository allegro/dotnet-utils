using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

internal class PrintSamplingConfigurationService : IHostedService
{
    private readonly SamplingConfig _samplingConfig;
    private readonly ILogger<PrintSamplingConfigurationService> _logger;

    public PrintSamplingConfigurationService(
        IOptions<SamplingConfig> samplingConfig,
        ILogger<PrintSamplingConfigurationService> logger)
    {
        _samplingConfig = samplingConfig.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
#pragma warning disable CA1848
        _logger.LogInformation("SamplingConfig: {@config}", _samplingConfig);
#pragma warning restore CA1848
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}