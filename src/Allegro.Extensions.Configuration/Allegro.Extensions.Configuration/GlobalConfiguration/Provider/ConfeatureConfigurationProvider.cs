using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Allegro.Extensions.Configuration.Client;
using Allegro.Extensions.Configuration.Configuration;
using Allegro.Extensions.Configuration.Exceptions;
using Allegro.Extensions.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Allegro.Extensions.Configuration.GlobalConfiguration.Provider;

/// <summary>
/// Global configuration provider. Can be used to read the global configuration from the files in the path given
/// by the <see cref="ContextGroupsConfiguration"/> or using the fallback service.
/// </summary>
internal class ConfeatureConfigurationProvider : ConfigurationProvider, ITraversableChainedConfigurationProviderWrapper
{
    internal static readonly string AllServicesMarker = "*";
    internal static readonly string ServiceMetadataKeySuffix = "metadata:service";

    private readonly ContextGroupsConfiguration _configuration;

    private readonly IConfeatureServiceClient? _confeatureClient;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly string _serviceName;
    private readonly bool _loadAllContexts;
    private readonly ISet<string> _loadedSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public IConfigurationRoot ConfigurationRoot { get; private set; } =
        new ConfigurationRoot(new List<IConfigurationProvider>());
    public DeferredConfeatureLogger ToBeLogged { get; } = new();
    private ILogger DeferredLogger => ToBeLogged;

    public ConfeatureConfigurationProvider(
        ContextGroupsConfiguration configuration,
        HttpClient? httpClient,
        bool isDevelopment,
        ConfeatureOptions confeatureOptions)
    {
        _configuration = configuration;
        Random jitter = new();
        _retryPolicy = Policy
            .Handle<RestEase.ApiException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.5, retryAttempt))
                                                       + TimeSpan.FromMilliseconds(jitter.Next(0, 300)));

        _confeatureClient = httpClient != null ? new ConfeatureServiceClient(httpClient) : null;
        _loadAllContexts = isDevelopment; // TODO || cloudAppInfo.IntegrationTesting;
        _serviceName = confeatureOptions.ServiceName switch
        {
            // on local machine or in integration tests, fallback to fetching all global contexts
            null when _loadAllContexts => string.Empty,
            // on the cluster we expect ApplicationName to be present
            null => throw new InvalidOperationException(
                $"{nameof(confeatureOptions.ServiceName)} is required but has no value."),
            _ => confeatureOptions.ServiceName
        };
    }

    // These attributes will be useful when we turn on the analyzers
    [SuppressMessage(
        "Usage",
        "VSTHRD002",
        MessageId = "Avoid problematic synchronous waits",
        Justification = "Cannot use await in a overriding void method")]
    [SuppressMessage(
        "Async",
        "AsyncifyInvocation",
        MessageId = "Use Task Async",
        Justification = "Cannot use await in a overriding void method")]
    public override void Load()
    {
        ConfigurationRoot = new ConfigurationRoot(LoadContexts());

        // the providers are iterated to prevent duplicates - dictionary will not allow them
        Data = ConfigurationRoot
            .Providers
            .Aggregate(
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                (dict, provider) =>
                    dict
                        .Concat(
                            provider.GetFullKeyNames()
                                .Select(
                                    key =>
                                    {
                                        provider.TryGet(key, out var value);
                                        return new KeyValuePair<string, string>(
                                            key,
                                            value);
                                    }))
                        .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase));
    }

    private IList<IConfigurationProvider> LoadContexts()
    {
        if (string.IsNullOrEmpty(_serviceName) && _confeatureClient != null)
        {
            // locally in Development always use fallback service
            return LoadFromFallbackService().GetAwaiter().GetResult();
        }

        var invalidContextGroup = _configuration.ContextGroups.Find(cg => !Directory.Exists(cg.Path));
        if (invalidContextGroup is not null)
        {
#pragma warning disable CA1848
            DeferredLogger.LogError(
                "[Confeature] Could not find directory: {InvalidContextGroupPath} " +
                "for context group: {InvalidContextGroupName}",
                invalidContextGroup.Path,
                invalidContextGroup.Name);
#pragma warning restore CA1848
            return LoadFromFallbackService().GetAwaiter().GetResult();
        }

        try
        {
            var configurationProviders = new List<IConfigurationProvider>();
            foreach (var contextGroup in _configuration.ContextGroups)
            {
                foreach (var fileName in Directory.EnumerateFiles(
                             contextGroup.Path,
                             "*.json",
                             SearchOption.AllDirectories))
                {
                    // ignore hidden OS files
                    if (fileName.Split(Path.DirectorySeparatorChar).Any(part => part.StartsWith(".", StringComparison.Ordinal)))
                    {
                        continue;
                    }

                    // ignore contexts that current service is not subscribing to
                    if (!string.IsNullOrEmpty(_serviceName) &&
                        !ConfigurationContextExtensions.IsServiceListedForContext(
                            File.OpenRead(fileName),
                            _serviceName))
                    {
                        continue;
                    }

                    configurationProviders.Add(LoadFromFile(fileName, contextGroup.Name));
                }
            }

            return configurationProviders;
        }
        catch (IOException e)
        {
#pragma warning disable CA1848
            DeferredLogger.LogError(
                e,
                "Caught an exception when trying to read the global config, falling back to the HTTP");
#pragma warning restore CA1848
            return LoadFromFallbackService().GetAwaiter().GetResult();
        }
    }

    private async Task<IList<IConfigurationProvider>> LoadFromFallbackService()
    {
        if (_confeatureClient == null)
        {
            throw new ConfeatureClientNotConfiguredException();
        }

#pragma warning disable CA1848
        DeferredLogger.LogInformation("[Confeature] Using fallback service to retrieve the configuration");
#pragma warning restore CA1848

        var globalConfig = await _retryPolicy.ExecuteAsync(() => _confeatureClient.GetGlobalConfiguration(_serviceName));

        var taskList = new List<Task<(Stream, string, string)>>(globalConfig.ContextGroups.Sum(cg => cg.Contexts.Count));

        foreach (var contextGroup in globalConfig.ContextGroups)
        {
            foreach (var context in contextGroup.Contexts)
            {
                taskList.Add(
                    _retryPolicy.ExecuteAsync(
                        async () => (await _confeatureClient.GetGlobalConfigurationContext(contextGroup.Name, context),
                            context,
                            contextGroup.Name)));
            }
        }

        await Task.WhenAll(taskList);

        var configurationProviders = new List<IConfigurationProvider>();
        foreach (var task in taskList)
        {
            var (configStream, contextName, contextGroupName) = await task;
            var contextDict = JsonConfigurationFileParser.Parse(configStream);

            configurationProviders.Add(
                new ConfeatureContextConfigurationProvider(
                    ToDictionaryWithContextPrefixedKeys(contextDict, contextName),
                    contextName,
                    contextGroupName));
        }

#pragma warning disable CA1848
        DeferredLogger.LogInformation("[Confeature] Configuration loaded from the fallback service successfully");
#pragma warning restore CA1848

        return configurationProviders;
    }

    private IConfigurationProvider LoadFromFile(string fileName, string contextGroup)
    {
        return new ConfeatureContextConfigurationProvider(
            ToDictionaryWithContextPrefixedKeys(
                JsonConfigurationFileParser.Parse(File.OpenRead(fileName)),
                fileName.ToContextName()),
            fileName.ToContextName(),
            contextGroup);
    }

    private IDictionary<string, string> ToDictionaryWithContextPrefixedKeys(
        IDictionary<string, string> dict,
        string context)
    {
        string RemovePrefix(string str, string prefix) =>
            str.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? str[prefix.Length..]
                : str;

        if (!dict.TryGetValue("metadata:sectionName", out var sectionName) || string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = context;
        }

        if (_loadedSections.Contains(sectionName))
        {
            throw new GlobalContextSectionNameConflictException(context, sectionName);
        }

        _loadedSections.Add(sectionName);

        var metadataToAdd = dict
            .Where(
                x => x.Key.StartsWith("metadata:deploymentInfo", StringComparison.OrdinalIgnoreCase) ||
                     (x.Key.StartsWith("metadata:services", StringComparison.OrdinalIgnoreCase) &&
                      (x.Value == _serviceName || x.Value == AllServicesMarker)));

        var retDct = dict
            .Where(x => !x.Key.StartsWith("metadata", StringComparison.OrdinalIgnoreCase))
            .Union(metadataToAdd)
            .Select(x => x.Key.StartsWith("metadata:services", StringComparison.OrdinalIgnoreCase)
                ? (ServiceMetadataKeySuffix, _serviceName)
                : (x.Key, x.Value))
            .Select(x => ($"{sectionName}:{RemovePrefix(x.Item1, "config:")}", x.Item2))
            .ToDictionary(x => x.Item1, x => x.Item2, StringComparer.OrdinalIgnoreCase);

        if (_loadAllContexts)
        {
            retDct[$"{sectionName}:{ServiceMetadataKeySuffix}"] = "*";
        }

        return retDct;
    }
}