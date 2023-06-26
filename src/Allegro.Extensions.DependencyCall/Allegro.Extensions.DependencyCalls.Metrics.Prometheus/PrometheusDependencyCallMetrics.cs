using System.Diagnostics;
using Allegro.Extensions.DependencyCalls.Abstractions;
using Prometheus;

namespace Allegro.Extensions.DependencyCalls.Metrics.Prometheus;

internal class PrometheusDependencyCallMetrics : IDependencyCallMetrics
{
    private readonly Histogram _dependencyCallDuration;

    public PrometheusDependencyCallMetrics(IMetricFactory metrics, ApplicationNameProvider applicationNameProvider)
    {
        _dependencyCallDuration = DependencyCallDurationFactory(metrics, applicationNameProvider.ApplicationName);
    }

    private static Histogram DependencyCallDurationFactory(IMetricFactory metrics, string applicationName) =>
        metrics.CreateHistogram(
            $"{applicationName}_dependency_call_duration_metrics",
            "Duration of dependency call",
            new[] { "dependencyCallName", "type" });

    public void Succeeded(IRequest request, Stopwatch timer)
    {
        _dependencyCallDuration.WithLabels(request.GetType().FullName!, "succeeded")
            .Observe(timer.Elapsed.TotalSeconds);
    }

    public void Failed(IRequest request, Exception exception, Stopwatch timer)
    {
        _dependencyCallDuration.WithLabels(request.GetType().FullName!, "failed")
            .Observe(timer.Elapsed.TotalSeconds);
    }

    public void Fallback(IRequest request, Exception exception, Stopwatch timer)
    {
        _dependencyCallDuration.WithLabels(request.GetType().FullName!, "fallback")
            .Observe(timer.Elapsed.TotalSeconds);
    }

    public void Timeout(IRequest request, Stopwatch timer)
    {
        _dependencyCallDuration.WithLabels(request.GetType().FullName!, "timeout")
            .Observe(timer.Elapsed.TotalSeconds);
    }
}

internal record ApplicationNameProvider(string ApplicationName);