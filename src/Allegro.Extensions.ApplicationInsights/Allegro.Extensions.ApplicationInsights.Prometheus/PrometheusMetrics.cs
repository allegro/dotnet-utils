using Prometheus;

namespace Allegro.Extensions.ApplicationInsights.Prometheus;

internal class PrometheusMetrics
{
    private static readonly string[] DependencyLabelNames =
    {
        "service", "type", "target", "name", "operation_name", "success", "resultCode"
    };

    private static readonly string[] RequestLabelNames = { "service", "name", "success", "resultCode" };

    public Histogram ApplicationInsightsDependencyDuration { get; }
    public Histogram ApplicationInsightsRequestDuration { get; }

    public PrometheusMetrics(IMetricFactory? metrics = null)
    {
        metrics ??= Metrics.WithCustomRegistry(Metrics.DefaultRegistry);
        ApplicationInsightsDependencyDuration = DependencyHistogramFactory(metrics);
        ApplicationInsightsRequestDuration = RequestHistogramFactory(metrics);
    }

    private static Histogram DependencyHistogramFactory(IMetricFactory metrics)
    {
        return metrics.CreateHistogram(
            "ai_dependency_duration_seconds",
            "The duration of dependency call",
            new HistogramConfiguration
            {
                LabelNames = DependencyLabelNames,
                Buckets = new[] { 0.008, 0.016, 0.032, 0.064, 0.128, 0.512, 1, 4, 16 }
            });
    }

    private static Histogram RequestHistogramFactory(IMetricFactory metrics)
    {
        return metrics.CreateHistogram(
            "ai_request_duration_seconds",
            "The duration of request",
            new HistogramConfiguration
            {
                LabelNames = RequestLabelNames,
                Buckets = new[] { 0.008, 0.016, 0.032, 0.064, 0.128, 0.512, 1, 4, 16 }
            });
    }
}