namespace Allegro.Extensions.Identifiers.Abstractions;

/// <summary>
/// Marker interface. Indicates if class should be treated as strongly typed identifier.
/// Meziantou.Framework.StronglyTypedId that is recommended as a code generator implements this interface by default
/// </summary>
public interface IStronglyTypedId<out T>
{
    /// <summary>
    /// Value of identifier
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// String value of identifier
    /// </summary>
    public string ValueAsString { get; }
}