using Allegro.Extensions.DependencyCalls.Abstractions;

namespace Allegro.Extensions.DependencyCalls.Exceptions;

/// <summary>
/// Missing dependency call implementation for request type
/// </summary>
public class MissingDependencyCallException : Exception
{
    /// <summary>
    /// Constructor
    /// </summary>
    public MissingDependencyCallException(Request request)
        : base($"Missing dependency call for request {request.GetType().FullName}")
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public MissingDependencyCallException(string message)
        : base(message)
    {
    }
}