namespace Allegro.Extensions.DependencyCalls.Abstractions;

/// <summary>
/// Allows to dispatch any dependency call with default pipeline.
/// </summary>
public interface IDependencyCallDispatcher
{
    /// <summary>
    /// Dispatches Request with default pipeline and support for metrics, fallbacks, retry policies.
    /// </summary>
    Task<TResult> Dispatch<TRequest, TResult>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : Request
        where TResult : Result;
}