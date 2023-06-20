using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS1591
namespace Allegro.Extensions.Dapper.Postgres.Abstractions;

/// <summary>
/// Dto database type.
/// </summary>
[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Purpose of that enum")]
public enum DbType
{
    Int,
    BigInt,
    Text,
    Date,
    Decimal,
    Guid,
}