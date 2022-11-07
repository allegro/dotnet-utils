namespace Allegro.Extensions.Identifiers.Abstractions;

/// <summary>
/// Marker interface. Indicates if type should be treated as a strongly typed identifier.
/// Meziantou.Framework.StronglyTypedId that is recommended as a code generator implements this interface by default  and provides common JSON serializers.
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