using Allegro.Extensions.Financials.Extensions;
using Allegro.Extensions.Financials.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Allegro.Extensions.Financials.Tests.Unit.Extensions;

public class MoneyExtensionsTests
{
    [Fact]
    public void Sum_Money_Items()
    {
        var moneys = new List<Money>
        {
            new(10),
            new(5),
            new(0),
        };

        var sum = moneys.Sum();
        sum.Should().NotBeNull();
        sum.Amount.Should().Be(15);
    }

    [Fact]
    public void Sum_Money_Items_with_null()
    {
        var moneys = new List<Money>
        {
            new(5),
            new(2),
        };

        var sum = moneys.Sum();
        sum.Should().NotBeNull();
        sum.Amount.Should().Be(7);
    }

    [Fact]
    public void Sum_Struct_with_money()
    {
        var moneys = new List<StructWithMoney>
        {
            new(5.10m),
            new(4.20m),
            new(-2),
        };

        var sum = moneys.Sum(x => x.Money);
        sum.Should().NotBeNull();
        sum.Amount.Should().Be(7.30m);
    }

    [Fact]
    public void Sum_Class_with_money()
    {
        var moneys = new List<ClassWithMoney>
        {
            new(5.10m),
            new(4.20m),
            new(-2),
        };

        var sum = moneys.Sum(x => x.Money);
        sum.Should().NotBeNull();
        sum.Amount.Should().Be(7.30m);
    }

    private readonly struct StructWithMoney
    {
        public Money Money { get; }

        public StructWithMoney(decimal money)
        {
            Money = new Money(money);
        }
    }

    private class ClassWithMoney
    {
        public Money Money { get; }

        public ClassWithMoney(decimal money)
        {
            Money = new Money(money);
        }
    }
}