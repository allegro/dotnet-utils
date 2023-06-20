#pragma warning disable CS1591

namespace Allegro.Extensions.Dapper.Postgres.Exceptions;

/// <summary>
/// Exception which indicates invalid underlying DbConnection type.
/// </summary>
public class InvalidDbConnectionTypeException : Exception
{
    public InvalidDbConnectionTypeException(string expectedType)
        : base($"Invalid DbConnectionType, expected type: {expectedType}")
    {
    }
}