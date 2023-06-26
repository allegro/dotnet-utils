namespace Allegro.Extensions.DependencyCalls.Polly.Exceptions;

/// <summary>
/// Missing dependency call implementation for request type
/// </summary>
public class MissingDependencyCallException : Exception
{
    /// <summary>
    /// Constructor
    /// </summary>
    public MissingDependencyCallException(Type requestType)
        : base($"Missing dependency call for request {requestType.FullName}")
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