using System;
using FluentAssertions;
using Xunit;

namespace Allegro.Extensions.Validators.Tests.Unit;

public class InputValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void EnsureHasValue_WhenInvalidInput_ShouldThrowArgumentNullException(string test)
    {
        var act = () => InputValidator.EnsureHasValue(test);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("testowy_string")]
    [InlineData("12333333")]
    public void EnsureHasValue_WhenValidInput_ShouldNotThrowAnyException(string value)
    {
        var act = () => InputValidator.EnsureHasValue(value);
        act.Should().NotThrow();
    }
}