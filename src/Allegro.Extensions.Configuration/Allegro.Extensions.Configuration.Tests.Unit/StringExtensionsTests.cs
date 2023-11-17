// using System;
// using FluentAssertions;
// using Vabank.Confeature.Extensions;
// using Xunit;
//
// namespace Vabank.Confeature.Tests.Unit;
//
// public class StringExtensionsTests
// {
//     [Theory]
//     [InlineData("c:/Program Files/context.env.json", "context")]
//     [InlineData("/Users/jan.dzban/Repos/global-config/platform.dev.json", "platform")]
//     [InlineData("./platform.dev.json", "platform")]
//     [InlineData("platform.dev.json", "platform")]
//     public void ShouldFetchContext_WhenFilePathIsValid(string filePath, string contextName)
//     {
//         filePath.ToContextName().Should().Be(contextName);
//     }
//
//     [Fact]
//     public void ShouldThrow_WhenFilePathIsNull()
//     {
//         // Arrange
//         string filePath = null;
//         var act = () => filePath.ToContextName();
//
//         // Act and assert
//         act.Should().Throw<ArgumentNullException>();
//     }
// }