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
            new HistogramConfiguration()
            {
                LabelNames = new[] { "dependencyCallName", "type" },
                Buckets = new[] { 0.008, 0.016, 0.032, 0.064, 0.128, 0.512, 1, 4, 16 }
            });

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