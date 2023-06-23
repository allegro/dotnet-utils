using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Allegro.Extensions.DependencyCall.Abstractions;

namespace Allegro.Extensions.DependencyCall;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Interface implementation")]
internal class NoOperationDependencyCallMetrics : IDependencyCallMetrics
{
    public void Total(IRequest request, Stopwatch timer)
    {
    }

    public void Succeeded(IRequest request, Stopwatch timer)
    {
    }

    public void Failed(IRequest request, Exception exception, Stopwatch timer)
    {
    }

    public void Fallback(IRequest request, Stopwatch timer)
    {
    }
}