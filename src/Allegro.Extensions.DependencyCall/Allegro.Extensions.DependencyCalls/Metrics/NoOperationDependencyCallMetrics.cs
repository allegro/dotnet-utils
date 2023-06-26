using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Allegro.Extensions.DependencyCalls.Abstractions;

namespace Allegro.Extensions.DependencyCalls.Metrics;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Interface implementation")]
internal class NoOperationDependencyCallMetrics : IDependencyCallMetrics
{
    public void Succeeded<TRequest, TResult>(TRequest request, Stopwatch timer)
        where TRequest : Request<TResult>
    {
    }

    public void Failed<TRequest, TResult>(TRequest request, Exception exception, Stopwatch timer)
        where TRequest : Request<TResult>
    {
    }

    public void Fallback<TRequest, TResult>(TRequest request, Exception exception, Stopwatch timer)
        where TRequest : Request<TResult>
    {
    }

    public void Timeout<TRequest, TResult>(TRequest request, Stopwatch timer)
        where TRequest : Request<TResult>
    {
    }
}