using Allegro.Extensions.DependencyCall.Abstractions;

namespace Allegro.Extensions.DependencyCall;

internal class NoOperationDependencyCallMetrics : IDependencyCallMetrics
{
    private static readonly NoOpTimer NoOpTimerInstance = new NoOpTimer();
    public void Requested(IRequest request)
    {
    }

    public void Executed(IRequest request)
    {
    }

    public void Failed(IRequest request, Exception exception)
    {
    }

    public void Fallback(IRequest request)
    {
    }

    public IDisposable StartTimer(IRequest request)
    {
        return NoOpTimerInstance;
    }

    private class NoOpTimer : IDisposable
    {
        public void Dispose()
        {
        }
    }
}