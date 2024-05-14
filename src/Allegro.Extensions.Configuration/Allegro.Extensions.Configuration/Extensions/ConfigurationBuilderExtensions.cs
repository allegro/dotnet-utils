using System.Net.Http;
using Allegro.Extensions.Configuration.Configuration;
using Allegro.Extensions.Configuration.GlobalConfiguration;
using Allegro.Extensions.Configuration.GlobalConfiguration.Provider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Allegro.Extensions.Configuration.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static ConfigurationManager AddGlobalConfiguration(
        this ConfigurationManager builder,
        ConfeatureOptions confeatureOptions,
        IHostEnvironment hostEnvironment,
        Func<HttpMessageHandler, HttpMessageHandler>? fallbackServiceHandler = null)
    {
        var contextGroupsConfig = builder.GetSection(ContextGroupsConfiguration.SectionName).Get<ContextGroupsConfiguration>()
#pragma warning disable CSE001
                                  ?? new ContextGroupsConfiguration();
#pragma warning restore CSE001
        var fallbackUri = builder.GetValue<string>("Confeature:FallbackUri");
        ((IConfigurationBuilder)builder).Add(
            new ConfeatureConfigurationSource(
                contextGroupsConfig,
                !string.IsNullOrEmpty(fallbackUri) ? new Uri(fallbackUri) : null,
                hostEnvironment.IsDevelopment(),
                fallbackServiceHandler,
                confeatureOptions));

        return builder;
    }
}