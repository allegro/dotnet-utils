using System.Diagnostics.CodeAnalysis;
using Allegro.Extensions.DependencyCall.Abstractions;

namespace Allegro.Extensions.DependencyCall;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Interface implementation")]
internal class NoOperationDependencyCallMetrics : IDependencyCallMetrics
{
    public void Succeeded(IRequest request, TimeSpan duration)
    {
    }

    public void Failed(IRequest request, Exception exception, TimeSpan duration)
    {
    }

    public void Fallback(IRequest request, TimeSpan duration)
    {
    }
}