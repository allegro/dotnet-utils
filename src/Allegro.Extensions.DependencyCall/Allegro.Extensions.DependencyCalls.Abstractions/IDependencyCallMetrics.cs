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
    public void Succeeded<TRequest>(TRequest request, Stopwatch timer)
        where TRequest : Request;

    /// <summary>
    /// Triggered when new dependency call used timeout
    /// </summary>
    public void Timeout<TRequest>(TRequest request, Stopwatch timer)
        where TRequest : Request;

    /// <summary>
    /// Triggered when new dependency call used fallback
    /// </summary>
    public void Fallback<TRequest>(TRequest request, Stopwatch timer)
        where TRequest : Request;
}