using System.Diagnostics;
using Allegro.Extensions.DependencyCall.Abstractions;
using Polly;

namespace Allegro.Extensions.DependencyCall;

internal interface IDependencyCall<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Run(
        TRequest request,
        IDependencyCallMetrics dependencyCallMetrics,
        CancellationToken? cancellationToken = null);
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
        CancellationToken? cancellationToken)
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(CancelAfter);
        cancellationToken ??= cancellationTokenSource.Token;

        var policy = CustomPolicy(cancellationToken.Value);

        var dependencyCallTimer = new Stopwatch();
        dependencyCallTimer.Start();
        try
        {
            var response = await policy.ExecuteAsync(token => Execute(request, token), cancellationToken.Value);
            dependencyCallMetrics.Succeeded(request, dependencyCallTimer);
            return response;
        }
        catch (Exception exception)
        {
            var fallbackValue = await Fallback(request, exception, cancellationToken.Value);
            if (fallbackValue.ShouldThrowOnError == ShouldThrowOnError.Yes)
            {
                dependencyCallMetrics.Failed(request, exception, dependencyCallTimer);
                throw;
            }

            dependencyCallMetrics.Fallback(request, dependencyCallTimer);
            return fallbackValue.Response;
        }
        finally
        {
            dependencyCallTimer.Stop();
        }
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
    protected abstract Task<(ShouldThrowOnError ShouldThrowOnError, TResponse Response)> Fallback(
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