using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Allegro.Extensions.DependencyCalls.Abstractions;

namespace Allegro.Extensions.DependencyCalls;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Interface implementation")]
internal class NoOperationDependencyCallMetrics : IDependencyCallMetrics
{
    public void Succeeded<TRequest>(TRequest request, Stopwatch timer)
        where TRequest : Request
    {
    }

    public void Timeout<TRequest>(TRequest request, Stopwatch timer)
        where TRequest : Request
    {
    }

    public void Fallback<TRequest>(TRequest request, Stopwatch timer)
        where TRequest : Request
    {
    }
}