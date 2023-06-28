namespace Allegro.Extensions.DependencyCall.Abstractions;

/// <summary>
/// Expose metrics collected by dispatcher. Allow to support own implementation of metrics
/// </summary>
public interface IDependencyCallMetrics
{
    /// <summary>
    /// Triggered when new dependency call was executed successfully
    /// </summary>
    public void Succeeded(IRequest request, TimeSpan duration);

    /// <summary>
    /// Triggered when new dependency call failed with error
    /// </summary>
    public void Failed(IRequest request, Exception exception, TimeSpan duration);

    /// <summary>
    /// Triggered when new dependency call used fallback
    /// </summary>
    public void Fallback(IRequest request, TimeSpan duration);
}