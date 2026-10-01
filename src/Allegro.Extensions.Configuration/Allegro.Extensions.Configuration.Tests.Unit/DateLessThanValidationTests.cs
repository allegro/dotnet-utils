using System.ComponentModel.DataAnnotations;
using Allegro.Extensions.Configuration.Validation;
using FluentAssertions;
using Xunit;

namespace Vabank.Confeature.Tests.Unit;

public class DateLessThanValidationTests
{
    [Fact]
    public void IsValidTest()
    {
        // arrange
#pragma warning disable CSE001
        var dateRange = new DateRangeTest
        {
            StartDate = new DateTimeOffset(new DateTime(2022, 02, 21, 14, 44, 0)),
            EndDate = new DateTimeOffset(new DateTime(2022, 02, 21, 15, 0, 0))
        };
#pragma warning restore CSE001

        var dateLessThanAttribute = new DateLessThanAttribute("EndDate");

        // act
        var validationResult = dateLessThanAttribute.GetValidationResult(
            dateRange.StartDate,
            new ValidationContext(dateRange));

        // assert
        validationResult.Should().BeSameAs(ValidationResult.Success);
    }

    [Fact]
    public void IsNotValidTest()
    {
        // arrange
#pragma warning disable CSE001
        var dateRange = new DateRangeTest
        {
            StartDate = new DateTimeOffset(new DateTime(2022, 03, 21, 14, 44, 0)),
            EndDate = new DateTimeOffset(new DateTime(2022, 02, 21, 15, 0, 0))
        };
#pragma warning restore CSE001

        var dateLessThanAttribute =
            new DateLessThanAttribute("EndDate") { ErrorMessage = "StartDate must be before EndDate" };

        // act
        var validationResult = dateLessThanAttribute.GetValidationResult(
            dateRange.StartDate,
            new ValidationContext(dateRange));

        // assert
        validationResult.Should().NotBeNull();
        validationResult!.ErrorMessage.Should().NotBeNull();
        validationResult!.ErrorMessage.Should().Be(dateLessThanAttribute.ErrorMessage);
    }

    [Fact]
    public void PropertyNotFoundTest()
    {
        // arrange
#pragma warning disable CSE001
        var dateRange = new DateRangeTest
        {
            StartDate = new DateTimeOffset(new DateTime(2022, 02, 21, 14, 44, 0)),
            EndDate = new DateTimeOffset(new DateTime(2022, 02, 21, 15, 0, 0))
        };
#pragma warning restore CSE001

        var dateLessThanAttribute = new DateLessThanAttribute("Foo");

        // act
        Action act = () => dateLessThanAttribute.GetValidationResult(
            dateRange.StartDate,
            new ValidationContext(dateRange));

        // assert
        act.Should().Throw<ArgumentException>().WithMessage("Property not found (Parameter 'Foo')");
    }

    [Fact]
    public void PropertyWrongTypeTest()
    {
        // arrange
#pragma warning disable CSE001
        var dateRange = new DateRangeTest
        {
            StartDate = new DateTimeOffset(new DateTime(2022, 02, 21, 14, 44, 0)),
            EndDate = new DateTimeOffset(new DateTime(2022, 02, 21, 15, 0, 0))
        };
#pragma warning restore CSE001

        var dateLessThanAttribute = new DateLessThanAttribute("WrongPropertyType");

        // act
        Action act = () => dateLessThanAttribute.GetValidationResult(
            dateRange.StartDate,
            new ValidationContext(dateRange));

        // assert
        act.Should().Throw<ArgumentException>().WithMessage("Property is not a DateTimeOffset (Parameter 'WrongPropertyType')");
    }

    [Fact]
    public void ValueWrongTypeTest()
    {
        // arrange
#pragma warning disable CSE001
        var dateRange = new DateRangeTest
        {
            StartDate = new DateTimeOffset(new DateTime(2022, 02, 21, 14, 44, 0)),
            EndDate = new DateTimeOffset(new DateTime(2022, 02, 21, 15, 0, 0))
        };
#pragma warning restore CSE001

        var dateLessThanAttribute = new DateLessThanAttribute("EndDate");

        // act
        Action act = () => dateLessThanAttribute.GetValidationResult(
            "wrong object type",
            new ValidationContext(dateRange));

        // assert
        act.Should().Throw<ArgumentException>().WithMessage("'wrong object type' is not a DateTimeOffset (Parameter 'value')");
    }

    private class DateRangeTest
    {
        public DateTimeOffset StartDate { get; init; }
        public DateTimeOffset EndDate { get; init; }
        public int WrongPropertyType { get; init; }
    }
}