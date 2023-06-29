using System.Collections.Concurrent;
using System.Diagnostics;
using Allegro.Extensions.DependencyCall.Abstractions;
using Polly;
using Polly.Timeout;
using Polly.Wrap;

namespace Allegro.Extensions.DependencyCall;

internal interface IDependencyCall<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Run(
        TRequest request,
        IDependencyCallMetrics dependencyCallMetrics,
        CancellationToken cancellationToken);
}

#pragma warning disable MA0049
/// <summary>
/// Abstraction to support any dependency call, that allows to declare some common aspects (ex. fallbacks, metrics, retries, timeout) of any dependency call.
/// </summary>
/// <typeparam name="TRequest">Type of request data</typeparam>
/// <typeparam name="TResponse">Type of response data</typeparam>
public abstract class DependencyCall<TRequest, TResponse> : IDependencyCall<TRequest, TResponse>
#pragma warning restore MA0049
    where TRequest : IRequest<TResponse>
{
    async Task<TResponse> IDependencyCall<TRequest, TResponse>.Run(
        TRequest request,
        IDependencyCallMetrics dependencyCallMetrics,
        CancellationToken cancellationToken)
    {
        var policy = BuildPolicy(CustomPolicy);

        var dependencyCallTimer = new Stopwatch();
        dependencyCallTimer.Start();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await policy.ExecuteAsync(token => Execute(request, token), cancellationToken);
            dependencyCallMetrics.Succeeded(request, dependencyCallTimer.Elapsed);
            return response;
        }
        catch (Exception exception)
        {
            var fallbackResult = await TryFallback(request, exception, cancellationToken);

            switch (fallbackResult.Result)
            {
                case FallbackResult.ValueResult<TResponse> result:
                    dependencyCallMetrics.Fallback(request, dependencyCallTimer.Elapsed);
                    return result.Value;

                default:
                    dependencyCallMetrics.Failed(request, exception, dependencyCallTimer.Elapsed);
                    if (fallbackResult.FallbackException is not null)
                    {
                        throw new FallbackExecutionException(request, exception);
                    }

                    throw;
            }
        }
        finally
        {
            dependencyCallTimer.Stop();
        }
    }

    private async Task<(FallbackResult Result, Exception? FallbackException)>
        TryFallback(TRequest request, Exception exception, CancellationToken cancellationToken)
    {
        try
        {
            var result = await Fallback(request, exception, cancellationToken);
            return (result, null);
        }
        catch (Exception fallbackException)
        {
            return (FallbackResult.NotSupported, fallbackException);
        }
    }

    private static readonly ConcurrentDictionary<string, IAsyncPolicy<TResponse>> Policies = new();

    private IAsyncPolicy<TResponse> BuildPolicy(IAsyncPolicy<TResponse> customPolicy)
    {
        var dependencyCallPolicyKey = GetType().FullName!;

        return
            PolicyConfiguration.CachePolicy
                ? Policies.GetOrAdd(
                    dependencyCallPolicyKey,
                    _ => BuildPolicyInternal())
                : BuildPolicyInternal();

        AsyncPolicyWrap<TResponse> BuildPolicyInternal() => Policy
            .TimeoutAsync<TResponse>(PolicyConfiguration.CancelAfter, PolicyConfiguration.TimeoutStrategy)
            .WrapAsync(customPolicy);
    }

    /// <summary>
    /// Main dependency call functionality - how to call external dependency that should be wrapped with dependency call abstraction.
    /// </summary>
    protected abstract Task<TResponse> Execute(TRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Fallback logic on any error
    /// </summary>
    /// <param name="request">Data send to dependency</param>
    /// <param name="exception">Thrown exception from dependency</param>
    /// <param name="cancellationToken">Cancellation token, as fallback might try to use other dependency.</param>
    /// <returns>Value object that informs about info how to handle error and what to return as a fallback response</returns>
    protected abstract Task<FallbackResult> Fallback(
        TRequest request,
        Exception exception,
        CancellationToken cancellationToken);

    private static readonly IAsyncPolicy<TResponse> NoOperation = Policy.NoOpAsync<TResponse>();

    /// <summary>
    /// Allows to preset custom retry policy (based on Polly). The policy is build and cached at first usage so should be static. Not able to reconfigure in runtime.
    /// </summary>
    protected virtual IAsyncPolicy<TResponse> CustomPolicy => NoOperation;

    /// <summary>
    /// Allows to set own timeout for call. The policy is build at first usage so timeout is set only once. Not able to reconfigure in runtime.
    /// </summary>
    protected virtual PolicyConfiguration PolicyConfiguration => PolicyConfiguration.Default;
}

/// <summary>
/// Allows to configure basic policy behaviour
/// </summary>
public record PolicyConfiguration
{
    private const int DefaultCancelCallAfterSeconds = 5;

    internal static PolicyConfiguration Default = new(
        cancelAfter: TimeSpan.FromSeconds(DefaultCancelCallAfterSeconds)
    );

    private PolicyConfiguration(
        TimeSpan cancelAfter,
        TimeoutStrategy timeoutStrategy = TimeoutStrategy.Pessimistic,
        bool cachePolicy = false)
    {
        CancelAfter = cancelAfter;
        TimeoutStrategy = timeoutStrategy;
        CachePolicy = cachePolicy;
    }

    /// <summary>Allows to set own timeout for call. The policy is build at first usage so timeout is set only once. Not able to reconfigure in runtime.</summary>
    internal TimeSpan CancelAfter { get; private init; }

    /// <summary>
    /// Default timeout strategy used by Polly.TimeouPolicy
    /// </summary>
    internal TimeoutStrategy TimeoutStrategy { get; private init; }

    /// <summary>
    /// Indicates if policy should be cached. True by default.
    /// </summary>
    internal bool CachePolicy { get; private init; }

    /// <summary>
    /// Able to reconfigure default cancel after timeout
    /// </summary>
    public PolicyConfiguration WithCancelAfter(TimeSpan cancelAfter) => this with { CancelAfter = cancelAfter };

    /// <summary>
    /// Able to reconfigure default timeout strategy
    /// </summary>
    public PolicyConfiguration WithTimeoutStrategy(TimeoutStrategy timeoutStrategy) =>
        this with { TimeoutStrategy = timeoutStrategy };

    /// <summary>
    /// Disable caching for this call
    /// </summary>
    public PolicyConfiguration WithoutCaching() =>
        this with { CachePolicy = false };
}

/// <summary>
/// Fallback execution result
/// </summary>
public abstract record FallbackResult
{
    /// <summary>
    /// Helper to get instance of NotSupported fallback result
    /// </summary>
    public static FallbackResult NotSupported => NotSupportedResult.Instance;

    /// <summary>
    /// Helper to get instance of result containing fallback value
    /// </summary>
    public static FallbackResult FromValue<TResponse>(TResponse response) => new ValueResult<TResponse>(response);

    internal record NotSupportedResult : FallbackResult
    {
        internal static NotSupportedResult Instance { get; } = new();

        private NotSupportedResult()
        {
        }
    }

    internal record ValueResult<TResponse>(TResponse Value) : FallbackResult;
}

internal class FallbackExecutionException : Exception
{
    public FallbackExecutionException(IRequest request, Exception dependencyCallException) : base(
        $"Error while executing fallback logic for request {request.GetType().Name}",
        dependencyCallException)
    {
    }
}