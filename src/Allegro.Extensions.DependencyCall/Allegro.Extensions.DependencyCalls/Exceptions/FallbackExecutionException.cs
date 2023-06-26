using Allegro.Extensions.DependencyCalls.Abstractions;

namespace Allegro.Extensions.DependencyCalls.Exceptions;

/// <summary>
/// Fallback execution exception
/// </summary>
internal class FallbackExecutionException : Exception
{
    public FallbackExecutionException(IRequest request, Exception dependencyCallException) : base(
        $"Error while executing fallback logic for request {request.GetType().Name}",
        dependencyCallException)
    {
    }
}