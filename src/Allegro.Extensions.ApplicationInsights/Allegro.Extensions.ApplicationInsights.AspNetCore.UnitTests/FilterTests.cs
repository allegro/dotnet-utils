using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging.Abstractions;

namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

public class FilterTests<TDependencyForFilter, TRequestForFilter>
    where TDependencyForFilter : DependencyForFilter
    where TRequestForFilter : RequestForFilter
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