namespace Allegro.Extensions.DependencyCalls.Abstractions;

/// <summary>
/// Dependency call base representation of request.
/// </summary>
public interface IRequest<TResponse> : IRequest
{
}

/// <summary>
/// Marker interface
/// </summary>
public interface IRequest
{
}