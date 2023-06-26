using Allegro.Extensions.DependencyCalls.Abstractions;
using Polly;
using Polly.Timeout;

namespace Allegro.Extensions.DependencyCalls.Polly;

internal interface IPollyDependencyCall<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Execute (happy path)
    /// </summary>
    public Task<TResponse> Execute(
        TRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Fallback (negative path)
    /// </summary>
    public Task<TResponse> Fallback(
        TRequest request,
        Exception exception,
        CancellationToken cancellationToken);

    /// <summary>
    /// A custom policies such as AsyncFallbackPolicy.
    /// </summary>
    public IAsyncPolicy<TResponse>[]? CustomResultPolicies { get; }

    /// <summary>
    /// A custom policies such as AsyncTimeoutPolicy, AsyncCircuitBreakerPolicy.
    /// </summary>
    public IAsyncPolicy[]? CustomPolicies { get; }

    /// <summary>
    /// Policy configuration
    /// </summary>
    public PollyPolicyConfiguration Configuration { get; }
}

/// <summary>
/// Abstraction to support any dependency call, that allows to declare some common aspects
/// (ex. fallbacks, metrics, retries, timeout) of any dependency call.
/// </summary>
public abstract class PollyDependencyCall<TRequest, TResponse> : IPollyDependencyCall<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Execute (happy path)
    /// </summary>
    public abstract Task<TResponse> Execute(
        TRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Fallback (negative path)
    /// </summary>
    public abstract Task<TResponse> Fallback(
        TRequest request,
        Exception exception,
        CancellationToken cancellationToken);

    /// <summary>
    /// A custom policies such as AsyncFallbackPolicy.
    /// </summary>
    public virtual IAsyncPolicy<TResponse>[]? CustomResultPolicies { get; }

    /// <summary>
    /// A custom policies such as AsyncTimeoutPolicy, AsyncCircuitBreakerPolicy.
    /// </summary>
    public virtual IAsyncPolicy[]? CustomPolicies { get; }

    /// <summary>
    /// Policy configuration
    /// </summary>
    public virtual PollyPolicyConfiguration Configuration { get; } = PollyPolicyConfiguration.Default;
}

/// <summary>
/// Fallback policy configuration
/// </summary>
public record PollyPolicyConfiguration(
    TimeSpan Timeout,
    TimeoutStrategy TimeoutStrategy = TimeoutStrategy.Pessimistic)
{
    private const double DefaultTimeoutInMilliseconds = 5000;

    /// <summary>
    /// Default configuration for policy
    /// </summary>
    public static PollyPolicyConfiguration Default =>
        new(TimeSpan.FromMilliseconds(DefaultTimeoutInMilliseconds));
}