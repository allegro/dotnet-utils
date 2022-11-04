namespace Allegro.Extensions.Identifiers.Abstractions;

public interface IStronglyTypedEntity<out T, Tu>
    where T : IStronglyTypedId<Tu>
{
    public T Id { get; }
}

public interface IStronglyTypedEntity<out T> : IStronglyTypedEntity<T, string>
    where T : IStronglyTypedId<string>
{
}