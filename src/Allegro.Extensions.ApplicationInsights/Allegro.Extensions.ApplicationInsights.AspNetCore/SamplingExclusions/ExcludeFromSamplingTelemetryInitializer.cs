using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StringToExpression.LanguageDefinitions;
// ReSharper disable InconsistentNaming

namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

internal static class LoggerExtensions
{
    private static readonly Action<ILogger, Exception> _applyingRulesFailed = LoggerMessage.Define(
        LogLevel.Warning,
        new EventId(1, nameof(ApplyingRulesFailed)),
        "Applying rules failed! Telemetry will be sampled!");

    private static readonly Action<ILogger, string, string, string, Exception> _compilingRulesFailed =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Error,
            new EventId(2, nameof(CompilingRulesFailed)),
            "Compiling rule \"{FilterKey}\" (\"{FilterValue}\") for {FilterType} failed! This rule will be not applied");

    public static void ApplyingRulesFailed(this ILogger logger, Exception exception)
    {
        _applyingRulesFailed(logger, exception);
    }

    public static void CompilingRulesFailed(this ILogger logger, Exception exception, string filterKey, string filterValue, string filterType)
    {
        _compilingRulesFailed(logger, filterKey, filterValue, filterType, exception);
    }
}

/// <summary>
/// It is publicly visible for unit tests purposes. See Demo.UnitTests
/// </summary>
public class ExcludeFromSamplingTelemetryInitializer<TDependencyForFilter, TRequestForFilter> : ITelemetryInitializer
    where TDependencyForFilter : DependencyForFilter
    where TRequestForFilter : RequestForFilter
{
    private readonly Func<DependencyTelemetry, TDependencyForFilter> _dependencyMap;
    private readonly List<Func<TDependencyForFilter, bool>> _dependencyPredicates;
    private readonly ILogger _logger;
    private readonly Func<RequestTelemetry, TRequestForFilter> _requestMap;
    private readonly List<Func<TRequestForFilter, bool>> _requestPredicates;

    /// <summary>
    /// It is publicly visible for unit tests purposes. See Demo.UnitTests
    /// </summary>
    public ExcludeFromSamplingTelemetryInitializer(
        IOptions<ExcludeFromSamplingTelemetryConfig> config,
        ITelemetryInitializerLogger logger,
        Func<DependencyTelemetry, TDependencyForFilter> dependencyMap,
        Func<RequestTelemetry, TRequestForFilter> requestMap)
    {
        _logger = logger;
        _dependencyMap = dependencyMap;
        _requestMap = requestMap;
        _dependencyPredicates =
            CreatePredicates<TDependencyForFilter>(_logger, config.Value.DependencyRules);
        _requestPredicates =
            CreatePredicates<TRequestForFilter>(_logger, config.Value.RequestRules);
    }

    /// <summary>
    /// It is publicly visible for unit tests purposes. See Demo.UnitTests
    /// </summary>
    public void Initialize(ITelemetry telemetry)
    {
        try
        {
            switch (telemetry)
            {
                case DependencyTelemetry dependencyTelemetry:
                    ApplyDependencyRules(dependencyTelemetry, _dependencyPredicates, _dependencyMap);
                    break;
                case RequestTelemetry requestTelemetry:
                    ApplyRequestRules(requestTelemetry, _requestPredicates, _requestMap);
                    break;
            }
        }
        catch (Exception e)
        {
            _logger.ApplyingRulesFailed(e);
        }
    }

    internal static List<Func<T, bool>> CreatePredicates<T>(ILogger logger, Dictionary<string, string>? filters)
    {
        return filters?.Select(filter => CreatePredicate<T>(filter, logger)).ToList() ?? new List<Func<T, bool>>();
    }

    private static Func<T, bool> CreatePredicate<T>(KeyValuePair<string, string> filter, ILogger logger)
    {
        bool AlwaysFalsePredicate(T p) => false;

        if (string.IsNullOrWhiteSpace(filter.Value))
        {
            return AlwaysFalsePredicate;
        }

        try
        {
            ODataFilterLanguage language = new();
            var expression = language.Parse<T>(filter.Value);

            return expression.Compile();
        }
        catch (Exception e)
        {
            logger.CompilingRulesFailed(e, filter.Key, filter.Value, typeof(T).Name);
            return AlwaysFalsePredicate;
        }
    }

    internal static void ApplyDependencyRules(
        DependencyTelemetry dependencyTelemetry,
        List<Func<TDependencyForFilter, bool>> predicates,
        Func<DependencyTelemetry, TDependencyForFilter> dependencyMap)
    {
        var shouldExclude = predicates.Any(predicate => predicate(dependencyMap(dependencyTelemetry)));

        ((ISupportSampling)dependencyTelemetry).SamplingPercentage = shouldExclude ? 100 : null;
    }

    internal static void ApplyRequestRules(
        RequestTelemetry requestTelemetry,
        List<Func<TRequestForFilter, bool>> predicates,
        Func<RequestTelemetry, TRequestForFilter> requestMap)
    {
        var shouldExclude = predicates.Any(predicate => predicate(requestMap(requestTelemetry)));

        ((ISupportSampling)requestTelemetry).SamplingPercentage = shouldExclude ? 100 : null;
    }
}