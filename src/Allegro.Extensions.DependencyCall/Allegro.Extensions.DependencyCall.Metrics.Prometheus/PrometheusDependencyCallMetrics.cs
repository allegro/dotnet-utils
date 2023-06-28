using Allegro.Extensions.DependencyCall.Abstractions;
using Prometheus;

namespace Allegro.Extensions.DependencyCall.Metrics.Prometheus;

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
    public void Succeeded(IRequest request, TimeSpan duration)
    {
        _dependencyCallDuration.WithLabels(request.GetType().FullName!, "succeeded")
            .Observe(duration.TotalSeconds);
    }

    public void Failed(IRequest request, Exception exception, TimeSpan duration)
    {
        _dependencyCallDuration.WithLabels(request.GetType().FullName!, "failed").Observe(duration.TotalSeconds);
    }

    public void Fallback(IRequest request, TimeSpan duration)
    {
        _dependencyCallDuration.WithLabels(request.GetType().FullName!, "fallback")
            .Observe(duration.TotalSeconds);
    }
}

internal record ApplicationNameProvider(string ApplicationName);