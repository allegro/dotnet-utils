using System.Diagnostics;

namespace Allegro.Extensions.DependencyCalls.Abstractions;

/// <summary>
/// Expose metrics collected by dispatcher. Allow to support own implementation of metrics
/// </summary>
public interface IDependencyCallMetrics
{
    /// <summary>
    /// Triggered when new dependency call was executed successfully
    /// </summary>
    public void Succeeded<TRequest, TResult>(TRequest request, Stopwatch timer)
        where TRequest : Request<TResult>;

    /// <summary>
    /// Triggered when new dependency call failed with error
    /// </summary>
    public void Failed<TRequest, TResult>(TRequest request, Exception exception, Stopwatch timer)
        where TRequest : Request<TResult>;

    /// <summary>
    /// Triggered when new dependency call used fallback
    /// </summary>
    public void Fallback<TRequest, TResult>(TRequest request, Exception exception, Stopwatch timer)
        where TRequest : Request<TResult>;

    /// <summary>
    /// Triggered when new dependency call used timeout
    /// </summary>
    public void Timeout<TRequest, TResult>(TRequest request, Stopwatch timer)
        where TRequest : Request<TResult>;
}