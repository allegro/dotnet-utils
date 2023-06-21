namespace Allegro.Extensions.DependencyCall.Abstractions;

/// <summary>
/// Expose metrics collected by dispatcher. Allow to support own implementation of metrics
/// </summary>
public interface IDependencyCallMetrics
{
    /// <summary>
    /// Triggered when new dependency call is requested
    /// </summary>
    public void Requested(IRequest request);

    /// <summary>
    /// Triggered when new dependency call was executed successfully
    /// </summary>
    public void Executed(IRequest request);

    /// <summary>
    /// Triggered when new dependency call failed with error
    /// </summary>
    public void Failed(IRequest request, Exception exception);

    /// <summary>
    /// Triggered when new dependency call used fallback
    /// </summary>
    public void Fallback(IRequest request);

    /// <summary>
    /// Used to start timer for histograms. Dispose will be called at the end of call execution
    /// </summary>
    public IDisposable StartTimer(IRequest request);
}