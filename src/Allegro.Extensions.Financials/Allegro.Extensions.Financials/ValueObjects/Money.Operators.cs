using System;
#pragma warning disable CS1591

namespace Allegro.Extensions.Financials.ValueObjects;

public partial record Money
{
    // Money & Money
    public static Money operator +(Money? m1, Money? m2)
        => Calculate(m1?.Amount, m1?.Currency, m2?.Amount, m2?.Currency, CalculateOperator.Add);

    public static Money operator -(Money? m1, Money? m2)
        => Calculate(m1?.Amount, m1?.Currency, m2?.Amount, m2?.Currency, CalculateOperator.Subtract);

    public static Money operator *(Money? m1, Money? m2)
        => Calculate(m1?.Amount, m1?.Currency, m2?.Amount, m2?.Currency, CalculateOperator.Multiply);

    public static Money operator /(Money? m1, Money? m2)
        => Calculate(m1?.Amount, m1?.Currency, m2?.Amount, m2?.Currency, CalculateOperator.Divide);

    // Money & decimal?
    public static Money operator +(Money? m1, decimal? d2)
        => Calculate(m1?.Amount, m1?.Currency, d2, c2: null, CalculateOperator.Add);

    public static Money operator -(Money? m1, decimal? d2)
        => Calculate(m1?.Amount, m1?.Currency, d2, c2: null, CalculateOperator.Subtract);

    public static Money operator *(Money? m1, decimal? d2)
        => Calculate(m1?.Amount, m1?.Currency, d2, c2: null, CalculateOperator.Multiply);

    public static Money operator /(Money? m1, decimal? d2)
        => Calculate(m1?.Amount, m1?.Currency, d2, c2: null, CalculateOperator.Divide);

    // Money & int?
    public static Money operator +(Money? m1, int? d2)
        => Calculate(m1?.Amount, m1?.Currency, d2, c2: null, CalculateOperator.Add);

    public static Money operator -(Money? m1, int? d2)
        => Calculate(m1?.Amount, m1?.Currency, d2, c2: null, CalculateOperator.Subtract);

    public static Money operator *(Money? m1, int? d2)
        => Calculate(m1?.Amount, m1?.Currency, d2, c2: null, CalculateOperator.Multiply);

    public static Money operator /(Money? m1, int? d2)
        => Calculate(m1?.Amount, m1?.Currency, d2, c2: null, CalculateOperator.Divide);

    // Money & Money
    public static bool operator <(Money? m1, Money? m2)
        => Compare(m1?.Amount, m1?.Currency, m2?.Amount, m2?.Currency, CompareOperator.LessThan);

    public static bool operator >(Money? m1, Money? m2)
        => Compare(m1?.Amount, m1?.Currency, m2?.Amount, m2?.Currency, CompareOperator.MoreThan);

    public static bool operator <=(Money? m1, Money? m2)
        => Compare(m1?.Amount, m1?.Currency, m2?.Amount, m2?.Currency, CompareOperator.LessOrEquals);

    public static bool operator >=(Money? m1, Money? m2)
        => Compare(m1?.Amount, m1?.Currency, m2?.Amount, m2?.Currency, CompareOperator.MoreOrEquals);

    // Money & decimal?
    public static bool operator <(Money? m1, decimal? d2)
        => Compare(m1?.Amount, m1?.Currency, d2, c2: null, CompareOperator.LessThan);

    public static bool operator >(Money? m1, decimal? d2)
        => Compare(m1?.Amount, m1?.Currency, d2, c2: null, CompareOperator.MoreThan);

    public static bool operator <=(Money? m1, decimal? d2)
        => Compare(m1?.Amount, m1?.Currency, d2, c2: null, CompareOperator.LessOrEquals);

    public static bool operator >=(Money? m1, decimal? d2)
        => Compare(m1?.Amount, m1?.Currency, d2, c2: null, CompareOperator.MoreOrEquals);

    private enum CalculateOperator
    {
        Add = 0, // +
        Subtract, // -
        Multiply, // *
        Divide, // /
    }

    private enum CompareOperator
    {
        LessThan, // <
        LessOrEquals, // <=
        MoreThan, // >
        MoreOrEquals, // >=
    }

    private static Money Calculate(
        decimal? d1,
        Currency? c1,
        decimal? d2,
        Currency? c2,
        CalculateOperator @operator)
    {
        d1 ??= 0;
        d2 ??= 0;
        c1 ??= c2 ?? Currency.PLN;
        c2 ??= c1;

        if (c1 != c2)
        {
            throw new Exception(
                $"Invalid currency! {c1} " +
                $"is different than {c2}");
        }

        var amount = @operator switch
        {
            CalculateOperator.Add => d1 + d2,
            CalculateOperator.Subtract => d1 - d2,
            CalculateOperator.Multiply => d1 * d2,
            CalculateOperator.Divide => d1 / d2,
            _ => throw new InvalidOperationException("Not supported calculate operator!")
        };

        return new Money(amount ?? 0, c1 ?? Currency.PLN);
    }

    private static bool Compare(
        decimal? d1,
        Currency? c1,
        decimal? d2,
        Currency? c2,
        CompareOperator @operator)
    {
        d1 ??= 0;
        d2 ??= 0;
        c1 ??= c2 ?? Currency.PLN;
        c2 ??= c1;

        if (c1 != c2)
        {
            throw new Exception($"Invalid currency! {c1} " +
                                $"is different than {c2}");
        }

        return @operator switch
        {
            CompareOperator.LessThan => d1 < d2,
            CompareOperator.LessOrEquals => d1 <= d2,
            CompareOperator.MoreThan => d1 > d2,
            CompareOperator.MoreOrEquals => d1 >= d2,
            _ => throw new InvalidOperationException("Not supported compare operator!")
        };
    }
}