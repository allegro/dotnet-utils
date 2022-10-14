using System;
using System.Collections.Generic;
using System.Linq;
using Allegro.Extensions.Financials.ValueObjects;

namespace Allegro.Extensions.Financials.Extensions;

public static class MoneyExtensions
{
    public static Money Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, Money> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        var sum = Money.Zero;
        return source.Aggregate(sum, (current, item) => current + selector(item));
    }

    public static Money Sum(this IEnumerable<Money> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var sum = Money.Zero;
        return source.Aggregate(sum, (current, item) => current + item);
    }

    public static Money? Round(this Money? money, int decimals, MidpointRounding midpointRounding)
    {
        if (money == null)
        {
            return null;
        }

        return new Money(
            Amount: Math.Round(money.Amount, decimals, midpointRounding),
            Currency: money.Currency);
    }
}