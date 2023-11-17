using System.Net.Http;
using Allegro.Extensions.Configuration.Configuration;
using Microsoft.Extensions.Configuration;

namespace Allegro.Extensions.Configuration.GlobalConfiguration.Provider;

/// <summary>
/// Source of global configuration key/values for the Confeature
/// </summary>
public sealed class ConfeatureConfigurationSource : IConfigurationSource
{
    private readonly ContextGroupsConfiguration _configuration;
    private readonly Uri? _fallbackUri;
    private readonly bool _isDevelopment;
    private readonly Func<HttpMessageHandler, HttpMessageHandler>? _authHandler;
    private readonly ConfeatureOptions _confeatureOptions;

    /// <param name="configuration">(Meta)configuration for thee configuration source</param>
    /// <param name="confeatureOptions">Confeature options</param>
    public ConfeatureConfigurationSource(
        ContextGroupsConfiguration configuration,
        ConfeatureOptions confeatureOptions)
    {
        _configuration = configuration;
        _confeatureOptions = confeatureOptions;
        _isDevelopment = false;
    }

    /// <param name="configuration">(Meta)configuration for thee configuration source</param>
    /// <param name="fallbackUri">URI of the fallback service that is used
    /// in case of the config map/file system issues</param>
    /// <param name="isDevelopment">Indicates whether the service is run on the local development environment</param>
    /// <param name="authHandler">Authentication handler to be used when connecting to the fallback config service</param>
    /// <param name="confeatureOptions">Confeature options</param>
    public ConfeatureConfigurationSource(
        ContextGroupsConfiguration configuration,
        Uri? fallbackUri,
        bool isDevelopment,
        Func<HttpMessageHandler, HttpMessageHandler>? authHandler,
        ConfeatureOptions confeatureOptions)
    {
        _configuration = configuration;
        _fallbackUri = fallbackUri;
        _isDevelopment = isDevelopment;
        _authHandler = authHandler;
        _confeatureOptions = confeatureOptions;
    }

    /// <summary>
    /// Builds the new Confeature configuration provider
    /// </summary>
    /// <returns>Confeature configuration provider</returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new ConfeatureConfigurationProvider(
            _configuration,
            Create(_fallbackUri, _authHandler),
            _isDevelopment,
            _confeatureOptions);
    }

    private static HttpClient? Create(Uri? uri, Func<HttpMessageHandler, HttpMessageHandler>? authHandler)
    {
        if (uri == null)
        {
            return null;
        }

        return new HttpClient(authHandler?.Invoke(new HttpClientHandler()) ?? new HttpClientHandler()) { BaseAddress = uri };
    }
}