using System.Diagnostics;
using Allegro.Extensions.DependencyCalls.Abstractions;
using Polly;
using Polly.Fallback;
using Polly.Timeout;
// ReSharper disable ArgumentsStyleNamedExpression

namespace Allegro.Extensions.DependencyCalls;

internal interface IDependencyCall<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Run(
        TRequest request,
        IDependencyCallMetrics dependencyCallMetrics,
        CancellationToken cancellationToken);
}

/// <summary>
/// Abstraction to support any dependency call, that allows to declare some common aspects
/// (ex. fallbacks, metrics, retries, timeout) of any dependency call.
/// </summary>
public abstract class DependencyCall<TRequest, TResponse> : IDependencyCall<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Execute (happy path)
    /// </summary>
    protected abstract Task<TResponse> Execute(
        TRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Fallback (negative path)
    /// </summary>
    protected abstract Task<TResponse> Fallback(
        TRequest request,
        Exception exception,
        CancellationToken cancellationToken);

    /// <summary>
    /// A custom policies such as AsyncFallbackPolicy.
    /// </summary>
    protected virtual IAsyncPolicy<TResponse>[]? CustomResultPolicies { get; }

    /// <summary>
    /// A custom policies such as AsyncTimeoutPolicy, AsyncCircuitBreakerPolicy.
    /// </summary>
    protected virtual IAsyncPolicy[]? CustomPolicies { get; }

    /// <summary>
    /// Policy configuration
    /// </summary>
    protected virtual PolicyConfiguration Configuration { get; } = PolicyConfiguration.Default;

    async Task<TResponse> IDependencyCall<TRequest, TResponse>.Run(
        TRequest request,
        IDependencyCallMetrics dependencyCallMetrics,
        CancellationToken cancellationToken)
    {
        var dependencyCallTimer = new Stopwatch();

        var timeoutPolicy = CreateTimeoutPolicy(
            Configuration.Timeout,
            Configuration.TimeoutStrategy,
            request,
            dependencyCallTimer,
            dependencyCallMetrics);

        var fallBackPolicy = CreateFallbackPolicy(
            Fallback,
            request,
            dependencyCallTimer,
            dependencyCallMetrics);

        var policies = fallBackPolicy.WrapAsync(timeoutPolicy);

        if (CustomPolicies?.Any() is true)
        {
            policies = policies.WrapAsync(Policy.WrapAsync(CustomPolicies));
        }

        if (CustomResultPolicies?.Any() is true)
        {
            policies = policies.WrapAsync(Policy.WrapAsync(CustomResultPolicies));
        }

        dependencyCallTimer.Start();
        var response = await policies.ExecuteAsync(
            async pollyCancellationToken =>
            {
                using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                    pollyCancellationToken,
                    cancellationToken);
                return await Execute(request, linkedTokenSource.Token);
            },
            CancellationToken.None);

        // CancellationToken.None above here indicates missing independent cancellation control and
        // cancellation will be provided by TimeoutPolicy.
        dependencyCallTimer.Stop();
        dependencyCallMetrics.Succeeded(request, dependencyCallTimer);
        return response;
    }

    private static AsyncFallbackPolicy<TResponse> CreateFallbackPolicy(
        Func<TRequest, Exception, CancellationToken, Task<TResponse>> fallback,
        TRequest request,
        Stopwatch dependencyCallTimer,
        IDependencyCallMetrics dependencyCallMetrics)
    {
        return Policy<TResponse>
            .Handle<Exception>()
            .FallbackAsync(
                fallbackAction: async (result, _, cancellationToken) =>
                {
                    try
                    {
                        var fallbackResponse = await fallback(request, result.Exception, cancellationToken);
                        dependencyCallMetrics.Fallback(request, result.Exception, dependencyCallTimer);
                        return fallbackResponse;
                    }
                    catch
                    {
                        dependencyCallMetrics.Failed(request, result.Exception, dependencyCallTimer);

                        // TODO: What is this logic about
                        // if (fallbackResult.FallbackException is not null)
                        // {
                        //     throw new FallbackExecutionException(request, result.Exception);
                        // }

                        throw;
                    }
                },
                onFallbackAsync: (_, _) => Task.CompletedTask); // The action to call asynchronously before invoking the fallback delegate
    }

    private static AsyncTimeoutPolicy CreateTimeoutPolicy(
        TimeSpan timeout,
        TimeoutStrategy timeoutStrategy,
        IRequest<TResponse> request,
        Stopwatch dependencyCallTimer,
        IDependencyCallMetrics dependencyCallMetrics)
    {
        return Policy
            .TimeoutAsync(
                timeout: timeout,
                timeoutStrategy: timeoutStrategy,
                onTimeoutAsync: (_, _, _) =>
                {
                    dependencyCallMetrics.Timeout(request, dependencyCallTimer);
                    return Task.CompletedTask;
                });
    }
}

/// <summary>
/// Fallback policy configuration
/// </summary>
public record PolicyConfiguration(
    TimeSpan Timeout,
    TimeoutStrategy TimeoutStrategy = TimeoutStrategy.Pessimistic)
{
    private const double DefaultTimeoutInMilliseconds = 5000;

    /// <summary>
    /// Default configuration for policy
    /// </summary>
    public static PolicyConfiguration Default =>
        new(TimeSpan.FromMilliseconds(DefaultTimeoutInMilliseconds));
}