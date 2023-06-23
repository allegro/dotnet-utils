using System.Diagnostics;
using Allegro.Extensions.DependencyCall.Abstractions;
using Polly;
using Polly.Timeout;

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
    private const int DefaultCancelCallAfterSeconds = 5;

    async Task<TResponse> IDependencyCall<TRequest, TResponse>.Run(
        TRequest request,
        IDependencyCallMetrics dependencyCallMetrics,
        CancellationToken cancellationToken)
    {
        var policy = BuildPolicy(CustomPolicy(cancellationToken));

        var dependencyCallTimer = new Stopwatch();
        dependencyCallTimer.Start();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await policy.ExecuteAsync(token => Execute(request, token), cancellationToken);
            dependencyCallMetrics.Succeeded(request, dependencyCallTimer);
            return response;
        }
        catch (Exception exception)
        {
            var fallbackResult = await TryFallback(request, exception, cancellationToken);

            switch (fallbackResult.Result)
            {
                case FallbackResult.ValueResult<TResponse> result:
                    dependencyCallMetrics.Fallback(request, dependencyCallTimer);
                    return result.Value;

                default:
                    dependencyCallMetrics.Failed(request, exception, dependencyCallTimer);
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

    private IAsyncPolicy<TResponse> BuildPolicy(IAsyncPolicy<TResponse> customPolicy)
    {
        return Policy.TimeoutAsync<TResponse>(CancelAfter, TimeoutStrategy.Pessimistic).WrapAsync(customPolicy);
    }

    /// <summary>
    /// Main dependency call functionality - how to call external dependency that should be wrapped with dependency call abstraction.
    /// </summary>
    protected abstract Task<TResponse> Execute(TRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Fallback login on any error
    /// </summary>
    /// <param name="request">Data send to dependency</param>
    /// <param name="exception">Thrown exception from dependency</param>
    /// <param name="cancellationToken">Cancellation token, as fallback might try to use other dependency.</param>
    /// <returns>Value object that informs about info how to handle error and what to return as a fallback response</returns>
    protected abstract Task<FallbackResult> Fallback(
        TRequest request,
        Exception exception,
        CancellationToken cancellationToken);

    /// <summary>
    /// Allows to preset custom retry policy (based on Polly).
    /// </summary>
    protected virtual IAsyncPolicy<TResponse> CustomPolicy(CancellationToken cancellationToken) =>
        Policy.NoOpAsync<TResponse>();

    /// <summary>
    /// Allows to set own timeout for call.
    /// </summary>
    protected virtual TimeSpan CancelAfter => TimeSpan.FromSeconds(DefaultCancelCallAfterSeconds);

    /// <summary>
    /// Expose possible actions when error occurs, by default fallback logic will be used
    /// </summary>
#pragma warning disable CS1591
    protected enum ShouldThrowOnError
    {
        Yes = 1,
        No = 2
    }
#pragma warning restore CS1591
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