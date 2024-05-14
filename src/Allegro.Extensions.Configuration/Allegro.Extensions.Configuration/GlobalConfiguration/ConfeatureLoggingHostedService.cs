using Allegro.Extensions.Configuration.Configuration;
using Allegro.Extensions.Configuration.GlobalConfiguration.Provider;
using Allegro.Extensions.Configuration.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Allegro.Extensions.Configuration.GlobalConfiguration;

public class ConfeatureLoggingHostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConfeatureOptions _confeatureOptions;

    public ConfeatureLoggingHostedService(
        ILogger<ConfeatureLoggingHostedService> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ConfeatureOptions confeatureOptions)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _confeatureOptions = confeatureOptions;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_confeatureOptions.IsEnabled)
        {
            return Task.CompletedTask;
        }

        if (_configuration is IConfigurationRoot configurationRoot &&
            configurationRoot.Providers
                .FirstOrDefault(
                    x =>
                        x is ConfeatureConfigurationProvider) is ConfeatureConfigurationProvider confeatureProvider)
        {
            foreach (var action in confeatureProvider.ToBeLogged.Deferred)
            {
                action(_logger);
            }
        }

        var configurationPrinter = _serviceProvider.GetRequiredService<IConfigurationPrinter>();
        var configurationResponse = configurationPrinter.GetConfiguration();

        foreach (var (providerId, providerMetadata) in configurationResponse.Providers)
        {
#pragma warning disable CA1848
            _logger.LogInformation("[Confeature] ConfigurationProvider {ProviderId}: {@ProviderMetadata}", providerId, providerMetadata);
#pragma warning restore CA1848
        }

        foreach (var (key, values) in configurationResponse.Configuration)
        {
            if (!values.Any())
            {
                continue;
            }

#pragma warning disable CA1848
            _logger.LogInformation("[Confeature] ConfigurationKey {Key}: {@Value}", key, values.First());
#pragma warning restore CA1848
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}