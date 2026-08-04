using System.Runtime.Serialization;
#pragma warning disable CS1591

namespace Allegro.Extensions.Financials.ValueObjects;

/// <summary>
/// Country currency codes (ISO 4217).
/// https://www.iban.com/currency-codes
/// </summary>
public enum Currency
{
    [EnumMember(Value = "PLN")]
    PLN = 985,

    [EnumMember(Value = "CZK")]
    CZK = 203,

    [EnumMember(Value = "EUR")]
    EUR = 978,

    [EnumMember(Value = "HUF")]
    HUF = 348,

    [EnumMember(Value = "USD")]
    USD = 840,
}