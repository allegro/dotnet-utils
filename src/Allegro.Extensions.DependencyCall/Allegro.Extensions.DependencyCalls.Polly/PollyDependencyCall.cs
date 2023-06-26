using Allegro.Extensions.DependencyCalls.Abstractions;
using Polly;
using Polly.Timeout;

namespace Allegro.Extensions.DependencyCalls.Polly;

/// <summary>
/// Abstraction to support any dependency call, that allows to declare some common aspects
/// (ex. fallbacks, metrics, retries, timeout) of any dependency call.
/// </summary>
public abstract class PollyDependencyCall<TRequest> : DependencyCall<TRequest>
    where TRequest : Request
{
    /// <summary>
    /// A custom policies such as AsyncCircuitBreakerPolicy
    /// (those policies have to be singleton so remember to override registration in DI)
    /// </summary>
    public IAsyncPolicy[]? CustomPolicies { get; set; }

    /// <summary>
    /// Policy configuration
    /// </summary>
    public PollyPolicyConfiguration Configuration { get; set; }
}

/// <summary>
/// Fallback policy configuration
/// </summary>
public record PollyPolicyConfiguration(
    TimeSpan Timeout,
    TimeoutStrategy TimeoutStrategy)
{
    private const double DefaultTimeoutInMilliseconds = 5000;
    private const TimeoutStrategy DefaultTimeoutStrategy = TimeoutStrategy.Pessimistic;

    /// <summary>
    /// Default configuration for policy
    /// </summary>
    public static PollyPolicyConfiguration Default =>
        new(TimeSpan.FromMilliseconds(DefaultTimeoutInMilliseconds), DefaultTimeoutStrategy);
}