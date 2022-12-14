#pragma warning disable CS1591
namespace Allegro.Extensions.Dapper.Postgres.Abstractions;

/// <summary>
/// Dto database type.
/// </summary>
public enum DbType
{
    Int,
    BigInt,
    Text,
    Date,
    Decimal,
    Guid,
}