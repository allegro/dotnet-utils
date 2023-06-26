using Allegro.Extensions.DependencyCalls.Abstractions;

namespace Allegro.Extensions.DependencyCalls.Exceptions;

/// <summary>
/// Fallback execution exception
/// </summary>
public class FallbackExecutionException : Exception
{
    /// <summary>
    /// Constructor
    /// </summary>
    public FallbackExecutionException(Request request, Exception dependencyCallException)
        : base(
            $"Error while executing fallback logic for request {request.GetType().Name}",
            dependencyCallException)
    {
    }
}