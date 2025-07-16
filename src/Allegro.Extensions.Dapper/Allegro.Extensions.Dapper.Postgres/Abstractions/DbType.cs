using System.Diagnostics.CodeAnalysis;

namespace Allegro.Extensions.Dapper.Postgres.Abstractions;

/// <summary>
/// Dto database type.
/// </summary>
[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Purpose of that enum")]
public enum DbType
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    Int,
    BigInt,
    Text,
    Date,
    Decimal,
    Guid,
#pragma warning restore CS1591
}