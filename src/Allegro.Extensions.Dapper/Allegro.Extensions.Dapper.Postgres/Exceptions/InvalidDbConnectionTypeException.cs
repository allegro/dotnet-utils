namespace Allegro.Extensions.Dapper.Postgres.Exceptions;

/// <summary>
/// Exception which indicates invalid underlying DbConnection type.
/// </summary>
public class InvalidDbConnectionTypeException(string expectedType)
    : Exception($"Invalid DbConnectionType, expected type: {expectedType}");