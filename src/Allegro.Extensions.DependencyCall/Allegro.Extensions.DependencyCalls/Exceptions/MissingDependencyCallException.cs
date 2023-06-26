using Allegro.Extensions.DependencyCalls.Abstractions;

namespace Allegro.Extensions.DependencyCalls.Exceptions;

/// <summary>
/// Missing dependency call implementation for request type
/// </summary>
internal class MissingDependencyCallException<T> : MissingDependencyCallException
{
    public MissingDependencyCallException(IRequest<T> query)
        : base($"Missing handler for query {query.GetType().FullName}")
    {
    }
}

internal class MissingDependencyCallException : Exception
{
    public MissingDependencyCallException(string message)
        : base(message)
    {
    }
}