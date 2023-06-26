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
    public void Succeeded(IRequest request, Stopwatch timer);

    /// <summary>
    /// Triggered when new dependency call failed with error
    /// </summary>
    public void Failed(IRequest request, Exception exception, Stopwatch timer);

    /// <summary>
    /// Triggered when new dependency call used fallback
    /// </summary>
    public void Fallback(IRequest request, Exception exception, Stopwatch timer);

    /// <summary>
    /// Triggered when new dependency call used timeout
    /// </summary>
    public void Timeout(IRequest request, Stopwatch timer);
}