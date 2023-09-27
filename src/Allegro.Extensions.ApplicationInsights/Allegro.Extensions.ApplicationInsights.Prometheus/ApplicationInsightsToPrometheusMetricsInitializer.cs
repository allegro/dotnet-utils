using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using Allegro.Extensions.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Allegro.Extensions.ApplicationInsights.Prometheus;

internal static class LoggerExtensions
{
    private static readonly Action<ILogger, string, Exception?> _invalidUrl = LoggerMessage.Define<string>(
        LogLevel.Warning,
        new EventId(1, nameof(InvalidUrl)),
        "Deparametrization of url path failed for {Url}! Invalid url.");

    private static readonly Action<ILogger, string, Exception> _deparametrizationFailed = LoggerMessage.Define<string>(
        LogLevel.Warning,
        new EventId(2, nameof(DeparametrizationFailed)),
        "Deparametrization of url path failed for {Url}!");

    private static readonly Action<ILogger, string, Exception?> _deparametrizationCircuitBreakerActivated =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(3, nameof(DeparametrizationCircuitBreakerActivated)),
            "Deparametrizator circuit breaker activated for {OriginalUrl}. " +
            "Extend deparametrizator algorithm to handle this URL, otherwise we could overload metrics server.");

    private static readonly Action<ILogger, Exception> _addingToPrometheusFailed =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(4, nameof(AddingToPrometheusFailed)),
            "Adding application insights telemetry to PrometheusMetrics failed!");

    public static void InvalidUrl(this ILogger logger, string url)
    {
        _invalidUrl(logger, url, null);
    }

    public static void DeparametrizationFailed(this ILogger logger, Exception exception, string url)
    {
        _deparametrizationFailed(logger, url, exception);
    }

    public static void DeparametrizationCircuitBreakerActivated(
        this ILogger logger,
        string originalUrl)
    {
        _deparametrizationCircuitBreakerActivated(logger, originalUrl, null);
    }

    public static void AddingToPrometheusFailed(this ILogger logger, Exception exception)
    {
        _addingToPrometheusFailed(logger, exception);
    }
}

internal static class UrlCircuitBreaker
{
    private static readonly ConcurrentDictionary<string, ImmutableHashSet<string>> _visitedUris = new();
    private static int _maxUrisPerHost = 100;

    /// <summary>
    /// Checks whether already exceeded max URIs count per given host and should break the circuit.
    /// </summary>
    internal static bool ShouldBreakCircuit(string host, string uri)
    {
        var hostVisitedUris = _visitedUris.AddOrUpdate(
            host,
            static (key, arg) => ImmutableHashSet.Create(arg),
            static (key, value, arg) => value.Count >= _maxUrisPerHost ? value : value.Add(arg),
            uri);

        return !hostVisitedUris.Contains(uri);
    }

    internal static void SetMaxUrisPerHost(int maxUrisPerHost)
    {
        _maxUrisPerHost = maxUrisPerHost;
    }
}

internal static class UrlDeparametrizator
{
#pragma warning disable MA0023
    private static readonly Regex Regex = new(@"(?=(\/)([0-9A-Fa-f\-]{36}|[\d]+)(\/|$))", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
#pragma warning restore MA0023
    internal static string? DeparametrizeUrlPath(string? url)
    {
        if (url == null)
            return null;

        var deparametrizedUrl = url;

        var matches = Regex.Matches(url);

        foreach (Match match in matches)
        {
            var toReplace = match.Groups[2];
            deparametrizedUrl = deparametrizedUrl.Replace(toReplace.Value, "{param}");
        }

        return deparametrizedUrl;
    }
}

internal class ApplicationInsightsToPrometheusMetricsInitializer : ITelemetryInitializer
{
    private const string AddedToPrometheus = "AddedToPrometheus";
    private readonly ILogger _logger;
    private readonly PrometheusMetrics _metrics;
    private readonly List<string> _dependenciesTypesIncludedLower;
    private readonly bool _includeBusRequests;
    private readonly ApplicationInsightsToPrometheusMetricsConfig _config;

    public ApplicationInsightsToPrometheusMetricsInitializer(
        IOptions<ApplicationInsightsToPrometheusMetricsConfig> config,
        PrometheusMetrics metrics,
        ITelemetryInitializerLogger logger)
    {
        _config = config.Value;
        _metrics = metrics;
        _logger = logger;
        _dependenciesTypesIncludedLower = config.Value?
                                              .DependenciesTypesIncluded
                                              .Select(p => p.ToLower(CultureInfo.InvariantCulture)).ToList() ??
                                          new List<string>();
        _includeBusRequests =
            config.Value?.IncludeBusRequests ?? false;
        UrlCircuitBreaker.SetMaxUrisPerHost(config.Value?.MaxUrisPerHost ?? 100);
    }

    private static string? Generalize(
        bool shouldGeneralizeUrl,
        string host,
        string? url,
        string alternativeUrl,
        ILogger logger)
    {
        string? result;
        if (shouldGeneralizeUrl)
        {
            var originalUrl = url;
            try
            {
                url = UrlDeparametrizator.DeparametrizeUrlPath(url);
            }
            catch (Exception e)
            {
                logger.DeparametrizationFailed(e, url ?? "null");
            }

            if (UrlCircuitBreaker.ShouldBreakCircuit(host, url ?? "null"))
            {
                logger.DeparametrizationCircuitBreakerActivated(originalUrl ?? "null");
                result = alternativeUrl;
            }
            else
            {
                result = url;
            }
        }
        else
        {
            result = alternativeUrl;
        }

        return result;
    }

    internal static void ApplyDependencyIncludes(
        DependencyTelemetry dependencyTelemetry,
        List<string> typesToInclude,
        bool shouldGeneralizeUrl,
        bool shouldGeneralizeOperationName,
        Action<string[], double> addToPrometheusMetrics,
        ILogger logger)
    {
        if (string.IsNullOrEmpty(dependencyTelemetry.Type))
            return;

        var type = dependencyTelemetry.Type.ToLower(CultureInfo.InvariantCulture);

        if (typesToInclude.Contains(type) &&
            !dependencyTelemetry.Properties.ContainsKey(AddedToPrometheus))
        {
            var name = dependencyTelemetry.Name;
            var operationName = dependencyTelemetry.Context?.Operation?.Name ?? string.Empty;

            if (type == "http")
            {
                var split = name?.Split(" ");

                if (split is { Length: 2 })
                {
                    var methodName = split[0];

                    name = Generalize(shouldGeneralizeUrl, dependencyTelemetry.Target, name, methodName, logger);
                }

                operationName = Generalize(
                    shouldGeneralizeOperationName,
                    "this",
                    operationName,
                    string.Empty,
                    logger);
            }

            var labels = new[]
            {
                dependencyTelemetry.Context?.Cloud?.RoleName ?? string.Empty,
                dependencyTelemetry.Type ?? string.Empty,
                dependencyTelemetry.Target ?? string.Empty,
                name ?? string.Empty,
                operationName ?? string.Empty,
                dependencyTelemetry.Success?.ToString() ?? string.Empty,
                dependencyTelemetry.ResultCode ?? string.Empty
            };

            addToPrometheusMetrics(labels, dependencyTelemetry.Duration.TotalSeconds);
            dependencyTelemetry.Properties.Add(AddedToPrometheus, "true");
        }
    }

    internal static void ApplyRequestsIncludes(
        RequestTelemetry requestTelemetry,
        bool includeBus,
        Action<string[], double> addToPrometheusMetrics)
    {
        if (string.IsNullOrEmpty(requestTelemetry.Name))
        {
            return;
        }

        if (includeBus && requestTelemetry.Name.StartsWith("Process", StringComparison.InvariantCulture) &&
            !requestTelemetry.Properties.ContainsKey(AddedToPrometheus))
        {
            var labels = new[]
            {
                requestTelemetry.Context?.Cloud?.RoleName ?? string.Empty,
                requestTelemetry.Name ?? string.Empty,
                requestTelemetry.Success?.ToString() ?? string.Empty,
                requestTelemetry.ResponseCode ?? string.Empty
            };

            addToPrometheusMetrics(labels, requestTelemetry.Duration.TotalSeconds);
            requestTelemetry.Properties.Add(AddedToPrometheus, "true");
        }
    }

    public void Initialize(ITelemetry telemetry)
    {
        try
        {
            switch (telemetry)
            {
                case DependencyTelemetry dependencyTelemetry:
                    ApplyDependencyIncludes(
                        dependencyTelemetry,
                        _dependenciesTypesIncludedLower,
                        _config.ShouldGeneralizeHttpDependencyTargetUrl,
                        _config.ShouldGeneralizeHttpDependencyOperationName,
                        (labels, duration) =>
                        {
                            _metrics.ApplicationInsightsDependencyDuration.WithLabels(labels)
                                .Observe(duration);
                        },
                        _logger);
                    break;
                case RequestTelemetry requestTelemetry:
                    ApplyRequestsIncludes(
                        requestTelemetry,
                        _includeBusRequests,
                        (labels, duration) =>
                        {
                            _metrics.ApplicationInsightsRequestDuration.WithLabels(labels)
                                .Observe(duration);
                        });
                    break;
            }
        }
        catch (Exception e)
        {
            _logger.AddingToPrometheusFailed(e);
        }
    }
}