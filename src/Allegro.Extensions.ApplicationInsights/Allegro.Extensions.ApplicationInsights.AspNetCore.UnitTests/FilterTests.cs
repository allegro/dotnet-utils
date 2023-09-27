using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging.Abstractions;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

public class FilterTests<TDependencyForFilter, TRequestForFilter>
{
    public void ApplyDependencyRules(
        DependencyTelemetry dependencyTelemetry,
        string filter,
        Func<DependencyTelemetry, TDependencyForFilter> dependencyMap)
    {
        var predicates =
            ExcludeFromSamplingTelemetryInitializer<TDependencyForFilter, TRequestForFilter>
                .CreatePredicates<TDependencyForFilter>(
                    NullLogger.Instance,
                    new Dictionary<string, string>() { { "any", filter } });

        ExcludeFromSamplingTelemetryInitializer<TDependencyForFilter, TRequestForFilter>.ApplyDependencyRules(
            dependencyTelemetry,
            predicates,
            dependencyMap);
    }

    public void ApplyRequestRules(
        RequestTelemetry requestTelemetry, string filter, Func<RequestTelemetry, TRequestForFilter> requestMap)
    {
        var predicates =
            ExcludeFromSamplingTelemetryInitializer<TDependencyForFilter, TRequestForFilter>
                .CreatePredicates<TRequestForFilter>(
                    NullLogger.Instance,
                    new Dictionary<string, string>() { { "any", filter } });

        ExcludeFromSamplingTelemetryInitializer<TDependencyForFilter, TRequestForFilter>.ApplyRequestRules(
            requestTelemetry,
            predicates,
            requestMap);
    }
}