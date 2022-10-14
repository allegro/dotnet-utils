using System.Collections.Generic;
using Allegro.Extensions.Financials.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Allegro.Extensions.Financials.Tests.Unit.ValueObjects;

public class MoneyTests
{
    public static IEnumerable<object?[]> DataValidAddMoney => new[]
    {
        new object?[]
        {
            new Money(10),
            new Money(10),
            new Money(20),
        },
        new object?[]
        {
            new Money(10),
            null,
            new Money(10),
        },
        new object?[]
        {
            null,
            null,
            new Money(0),
        },
        new object?[]
        {
            null,
            new Money(10),
            new Money(10),
        },
    };

    public static IEnumerable<object?[]> DataValidAddDecimal => new[]
    {
        new object?[]
        {
            new Money(10),
            10M,
            new Money(20),
        },
        new object?[]
        {
            new Money(10),
            null,
            new Money(10),
        },
        new object?[]
        {
            null,
            null,
            new Money(0),
        },
        new object?[]
        {
            null,
            10M,
            new Money(10),
        },
    };

    public static IEnumerable<object?[]> DataValidSubtractMoney => new[]
    {
        new object?[]
        {
            new Money(100),
            new Money(10),
            new Money(90),
        },
        new object?[]
        {
            new Money(10),
            null,
            new Money(10),
        },
        new object?[]
        {
            null,
            null,
            new Money(0),
        },
        new object?[]
        {
            null,
            new Money(10),
            new Money(-10),
        },
    };

    public static IEnumerable<object?[]> DataValidSubtractDecimal => new[]
    {
        new object?[]
        {
            new Money(100),
            10M,
            new Money(90),
        },
        new object?[]
        {
            new Money(10),
            null,
            new Money(10),
        },
        new object?[]
        {
            null,
            null,
            new Money(0),
        },
        new object?[]
        {
            null,
            10M,
            new Money(-10),
        },
    };

    [Theory]
    [MemberData(nameof(DataValidAddMoney))]
    public void TestCalculations_Add_Money_Valid(Money m1, Money m2, Money expected)
    {
        var result = m1 + m2;
        result.Should().NotBeNull();
        result.Amount.Should().Be(expected.Amount);
        result.Currency.Should().Be(expected.Currency);
    }

    [Theory]
    [MemberData(nameof(DataValidAddDecimal))]
    public void TestCalculations_Add_Decimal_Valid(Money m1, decimal? d2, Money expected)
    {
        var result = m1 + d2;
        result.Should().NotBeNull();
        result.Amount.Should().Be(expected.Amount);
        result.Currency.Should().Be(expected.Currency);
    }

    [Theory]
    [MemberData(nameof(DataValidSubtractMoney))]
    public void TestCalculations_Subtract_Money_Valid(Money m1, Money m2, Money expected)
    {
        var result = m1 - m2;
        result.Should().NotBeNull();
        result.Amount.Should().Be(expected.Amount);
        result.Currency.Should().Be(expected.Currency);
    }

    [Theory]
    [MemberData(nameof(DataValidSubtractDecimal))]
    public void TestCalculations_Subtract_Decimal_Valid(Money m1, decimal? d2, Money expected)
    {
        var result = m1 - d2;
        result.Should().NotBeNull();
        result.Amount.Should().Be(expected.Amount);
        result.Currency.Should().Be(expected.Currency);
    }

    // TODO: Add tests for Multiply and Divide and compares !
}