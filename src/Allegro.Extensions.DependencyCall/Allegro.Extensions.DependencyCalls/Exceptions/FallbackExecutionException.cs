namespace Allegro.Extensions.DependencyCalls.Exceptions;

/// <summary>
/// Fallback execution exception
/// </summary>
public class FallbackExecutionException : Exception
{
    /// <summary>
    /// Constructor
    /// </summary>
    public FallbackExecutionException(Type requestType, Exception dependencyCallException)
        : base(
            $"Error while executing fallback logic for request {requestType.FullName}",
            dependencyCallException)
    {
    }
}