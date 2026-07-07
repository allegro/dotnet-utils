using System.Runtime.Serialization;

namespace Allegro.Extensions.Financials.ValueObjects;

/// <summary>
/// Country currency codes (ISO 4217).
/// https://www.iban.com/currency-codes
/// </summary>
public enum Currency
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [EnumMember(Value = "PLN")]
    PLN = 985,
#pragma warning restore CS1591
}