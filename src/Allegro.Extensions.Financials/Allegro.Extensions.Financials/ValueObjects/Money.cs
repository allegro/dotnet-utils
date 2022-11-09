namespace Allegro.Extensions.Financials.ValueObjects;

/// <summary>
/// Value object to store money related data in standardize way
/// </summary>
public partial record Money(
    decimal Amount,
    Currency Currency = Currency.PLN)
{
    /// <summary>
    /// Creates money with value 0 and default currency (PLN)
    /// </summary>
    public static Money Zero => new(0);

    /// <summary>
    /// Defualt string representation of money
    /// </summary>
    public override string ToString() => $"{Amount} {Currency}";
}