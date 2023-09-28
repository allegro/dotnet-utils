namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

/// <summary>
/// Advanced configuration for adaptive sampling
/// </summary>
public record AdaptiveSamplingConfig
{
    /// <summary>
    /// As sampling percentage varies, what is the minimum value we're allowed to set?
    /// </summary>
    public double? MinSamplingPercentage { get; init; }

    /// <summary>
    /// As sampling percentage varies, what is the maximum value we're allowed to set?
    /// </summary>
    public double? MaxSamplingPercentage { get; init; }

    /// <summary>
    /// The target rate of logical operations that the adaptive algorithm aims to collect on each server host.
    /// If your web app runs on many hosts, reduce this value so as to remain within your target rate of traffic at the Application Insights portal.
    /// </summary>
    public double? MaxTelemetryItemsPerSecond { get; init; }

    /// <summary>
    /// The amount of telemetry to sample when the app has started.
    /// Don't reduce this value while you're debugging.
    /// </summary>
    public double? InitialSamplingPercentage { get; init; }

    /// <summary>
    /// In the calculation of the moving average, this value specifies the weight that should be assigned to the most recent value. Use a value equal to or less than 1.
    /// Smaller values make the algorithm less reactive to sudden changes.
    /// </summary>
    public double? MovingAverageRatio { get; init; }

    /// <summary>
    /// The interval at which the current rate of telemetry is reevaluated. Evaluation is performed as a moving average.
    /// You might want to shorten this interval if your telemetry is liable to sudden bursts.
    /// </summary>
    public TimeSpan? EvaluationInterval { get; init; }

    /// <summary>
    /// When sampling percentage value changes, this value determines how soon after are we allowed to lower
    /// the sampling percentage again to capture less data?
    /// </summary>
    public TimeSpan? SamplingPercentageDecreaseTimeout { get; init; }

    /// <summary>
    /// When sampling percentage value changes, this value determines how soon after are we allowed to
    /// increase the sampling percentage again to capture more data?
    /// </summary>
    public TimeSpan? SamplingPercentageIncreaseTimeout { get; init; }
}