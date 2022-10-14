using System.Runtime.Serialization;

namespace Allegro.Extensions.Financials.ValueObjects;

/// <summary>
/// Country currency codes (ISO 4217).
/// https://www.iban.com/currency-codes
/// </summary>
public enum Currency
{
    [EnumMember(Value = "PLN")]
    PLN = 985,
}