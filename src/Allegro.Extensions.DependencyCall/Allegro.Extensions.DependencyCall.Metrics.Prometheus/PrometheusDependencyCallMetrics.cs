using Allegro.Extensions.DependencyCall.Abstractions;
using Prometheus;

namespace Allegro.Extensions.DependencyCall.Metrics.Prometheus;

internal class PrometheusDependencyCallMetrics : IDependencyCallMetrics
{
    private readonly Counter _dependencyCallCounter;
    private readonly Histogram _dependencyCallDuration;

    public PrometheusDependencyCallMetrics(IMetricFactory metrics, ApplicationNameProvider applicationNameProvider)
    {
        _dependencyCallCounter = DependencyCallCounterFactory(metrics, applicationNameProvider.ApplicationName);
        _dependencyCallDuration = DependencyCallDurationFactory(metrics, applicationNameProvider.ApplicationName);
    }

    private static Counter DependencyCallCounterFactory(IMetricFactory metrics, string applicationName) =>
        metrics.CreateCounter(
            $"{applicationName}_dependency_call_used_total",
            "Count of dependency calls with additional information about fallback usage and exceptions",
            new[] { "dependencyCallName", "status" });

    private static Histogram DependencyCallDurationFactory(IMetricFactory metrics, string applicationName) =>
        metrics.CreateHistogram(
            $"{applicationName}_dependency_call_duration_metrics",
            "Duration of dependency call",
            new[] { "dependencyCallName" });

    public void Requested(IRequest request)
    {
        _dependencyCallCounter.WithLabels(request.GetType().FullName!, "requested").Inc();
    }

    public void Executed(IRequest request)
    {
        _dependencyCallCounter.WithLabels(request.GetType().FullName!, "executed").Inc();
    }

    public void Failed(IRequest request, Exception exception)
    {
        _dependencyCallCounter.WithLabels(request.GetType().FullName!, "failed").Inc();
    }

    public void Fallback(IRequest request)
    {
        _dependencyCallCounter.WithLabels(request.GetType().FullName!, "fallback").Inc();
    }

    public IDisposable StartTimer(IRequest request)
    {
        return _dependencyCallDuration.WithLabels(request.GetType().FullName!).NewTimer();
    }
}

internal record ApplicationNameProvider(string ApplicationName);