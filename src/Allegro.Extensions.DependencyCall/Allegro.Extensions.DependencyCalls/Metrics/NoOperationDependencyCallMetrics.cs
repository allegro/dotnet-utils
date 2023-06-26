using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Allegro.Extensions.DependencyCalls.Abstractions;

namespace Allegro.Extensions.DependencyCalls.Metrics;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Interface implementation")]
internal class NoOperationDependencyCallMetrics : IDependencyCallMetrics
{
    public void Succeeded(IRequest request, Stopwatch timer)
    {
    }

    public void Failed(IRequest request, Exception exception, Stopwatch timer)
    {
    }

    public void Fallback(IRequest request, Exception exception, Stopwatch timer)
    {
    }

    public void Timeout(IRequest request, Stopwatch timer)
    {
    }
}