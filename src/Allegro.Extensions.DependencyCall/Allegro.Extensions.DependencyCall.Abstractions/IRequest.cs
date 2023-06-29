namespace Allegro.Extensions.DependencyCall.Abstractions;

/// <summary>
/// Marker interface. Used to recognize Dependency Call request data
/// </summary>
/// <typeparam name="T">Type of dependency call response data</typeparam>
public interface IRequest<T> : IRequest
{
}

/// <summary>
/// Marker interface. Used to recognize Dependency Call request data
/// </summary>
public interface IRequest { }