namespace Allegro.Extensions.Financials.ValueObjects;

public partial record Money(
    decimal Amount,
    Currency Currency = Currency.PLN)
{
    public static Money Zero => new(0);

    public override string ToString() => $"{Amount} {Currency}";
}