namespace Allegro.Extensions.Configuration.Exceptions;

public class InvalidProviderTypeException : Exception
{
    public InvalidProviderTypeException(Type type) : base($"Invalid type {type.Name}")
    {
    }
}