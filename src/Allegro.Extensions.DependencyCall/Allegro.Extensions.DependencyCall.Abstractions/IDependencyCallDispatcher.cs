namespace Allegro.Extensions.DependencyCall.Abstractions;

/// <summary>
/// Allows to dispatch any dependency call with default pipeline.
/// </summary>
public interface IDependencyCallDispatcher
{
    /// <summary>
    /// Dispatches IRequest with default pipeline and support for metrics, fallbacks, retry policies.
    /// </summary>
    /// <param name="request">Request data</param>
    /// <param name="cancellationToken">Optional cancellation token. If null default cancellation policy will be used</param>
    /// <typeparam name="TResponse">Type of response data</typeparam>
    Task<TResponse> Dispatch<TResponse>(IRequest<TResponse> request, CancellationToken? cancellationToken = null);
}