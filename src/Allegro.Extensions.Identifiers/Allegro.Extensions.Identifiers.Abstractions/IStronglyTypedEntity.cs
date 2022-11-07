namespace Allegro.Extensions.Identifiers.Abstractions;

/// <summary>
/// Marker interface for strongly typed entities
/// </summary>
public interface IStronglyTypedEntity<out T, Tu>
    where T : IStronglyTypedId<Tu>
{
    /// <summary>
    /// Strongly typed entity id
    /// </summary>
    public T Id { get; }
}

/// <summary>
/// Marker interface for strongly typed identifiers
/// </summary>
public interface IStronglyTypedEntity<out T> : IStronglyTypedEntity<T, string>
    where T : IStronglyTypedId<string>
{
}