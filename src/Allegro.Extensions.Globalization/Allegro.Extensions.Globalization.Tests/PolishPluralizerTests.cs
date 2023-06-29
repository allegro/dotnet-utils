using Allegro.Extensions.Globalization.Extensions;
using FluentAssertions;
using Xunit;

namespace Allegro.Extensions.Globalization.Tests
{
    public class PolishPluralizerTests
    {
        [Theory]
        [InlineData(0, "równych miesięcznych rat", "równa miesięczna rata", "równe miesięczne raty", "równych miesięcznych rat", "równych miesięcznych rat")]
        [InlineData(3, "równych miesięcznych rat", "równa miesięczna rata", "równe miesięczne raty", "równych miesięcznych rat", "równe miesięczne raty")]
        [InlineData(5, "równych miesięcznych rat", "równa miesięczna rata", "równe miesięczne raty", "równych miesięcznych rat", "równych miesięcznych rat")]
        [InlineData(10, "równych miesięcznych rat", "równa miesięczna rata", "równe miesięczne raty", "równych miesięcznych rat", "równych miesięcznych rat")]
        [InlineData(20, "równych miesięcznych rat", "równa miesięczna rata", "równe miesięczne raty", "równych miesięcznych rat", "równych miesięcznych rat")]
        [InlineData(1, "spłat", "spłata", "spłaty", "spłat", "spłata")]
        [InlineData(2, "spłat", "spłata", "spłaty", "spłat", "spłaty")]
        [InlineData(3, "spłat", "spłata", "spłaty", "spłat", "spłaty")]
        [InlineData(4, "spłat", "spłata", "spłaty", "spłat", "spłaty")]
        [InlineData(5, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(6, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(7, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(8, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(9, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(10, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(11, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(12, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(13, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(14, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(15, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(16, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(17, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(19, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(20, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(21, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(22, "spłat", "spłata", "spłaty", "spłat", "spłaty")]
        [InlineData(23, "spłat", "spłata", "spłaty", "spłat", "spłaty")]
        [InlineData(25, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(99, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(113, "spłat", "spłata", "spłaty", "spłat", "spłat")]
        [InlineData(124, "spłat", "spłata", "spłaty", "spłat", "spłaty")]
        [InlineData(127, "spłat", "spłata", "spłaty", "spłat", "spłat")]
#pragma warning disable CA1720
        public void Pluralize_SampleText_RendersProperty(int quantity, string none, string single, string two, string five, string expected)
#pragma warning restore CA1720
        {
            // act
            var result = quantity.Pluralize(
                single: single,
                two: two,
                five: five,
                none: none);

            // assert
            result.Should().BeEquivalentTo(expected);
        }
    }
}