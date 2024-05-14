using System.ComponentModel.DataAnnotations;
using Allegro.Extensions.Configuration.Validation;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Vabank.Confeature.Tests.Unit;

public class ValidateObjectAttributeTests
{
    [Fact]
    public void ShouldReturnValidResponse_ForValidConfigurations_WithList()
    {
        // Arrange & Act
        var isValid = ValidateConfigurationDto("ValidConfig", out TestConfig dto, out var validationResults);

        // Assert
        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
        dto.ConfigList.Should().HaveCount(2);
        dto.ConfigList![0].Should().BeEquivalentTo(new InnerConfig { SomeValue = 1, OtherValue = "a" });
        dto.ConfigList[1].Should().BeEquivalentTo(new InnerConfig { SomeValue = 2, OtherValue = "b" });
    }

    [Fact]
    public void ShouldReturnValidResponse_ForValidConfigurations_WithDictionary()
    {
        // Arrange & Act
        var isValid = ValidateConfigurationDto("ValidDictionary", out TestConfigWithDictionary dto, out var validationResults);

        // Assert
        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
        dto.ConfigDict.Should().HaveCount(2);
        dto.ConfigDict!["item1"].Should().BeEquivalentTo(new InnerConfig { SomeValue = 1, OtherValue = "a" });
        dto.ConfigDict["item2"].Should().BeEquivalentTo(new InnerConfig { SomeValue = 2, OtherValue = "b" });
    }

    [Fact]
    public void ShouldReturnNonValidResponse_ForInvalidConfigurations_WithList()
    {
        // Arrange & Act
        var isValid = ValidateConfigurationDto("InvalidConfig", out TestConfig dto, out var validationResults);

        // Assert
        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(1);
        validationResults.First().ErrorMessage.Should().ContainAll("Index [0]", "Index [1]");
    }

    [Fact]
    public void ShouldReturnNonValidResponse_ForInvalidConfigurations_WithDictionary()
    {
        // Arrange & Act
        var isValid = ValidateConfigurationDto("InvalidDictionary", out TestConfigWithDictionary dto, out var validationResults);

        // Assert
        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(1);
        validationResults.First().ErrorMessage.Should().ContainAll("Index [0]", "Index [1]");
    }

    [Fact]
    public void ShouldReturnValidResponse_ForConfiguration_WithEmptyLists()
    {
        // Arrange & Act
        var isValid = ValidateConfigurationDto("ConfigWithEmptyList", out TestConfig dto, out var validationResults);

        // Assert
        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
        dto.ConfigList.Should().BeNullOrEmpty();
    }

    [Fact]
    public void ShouldReturnValidResponse_ForConfiguration_WithEmptyDictionary()
    {
        // Arrange & Act
        var isValid = ValidateConfigurationDto("ConfigWithEmptyDictionary", out TestConfigWithDictionary dto, out var validationResults);

        // Assert
        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
        dto.ConfigDict.Should().BeNullOrEmpty();
    }

    [Fact]
    public void ShouldRespectSkipNullObjectValidationFlag()
    {
        // Arrange & Act
        var isValid = ValidateConfigurationDto("NullTestConfig", out NullTestConfig dto, out var validationResults);

        // Assert
        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
        dto.NullConfig.Should().BeNull();
    }

    private static bool ValidateConfigurationDto<T>(
        string subSectionName,
        out T dto,
        out List<ValidationResult> validationResults) where T : new()
    {
        dto = new T();
        validationResults = new List<ValidationResult>();

        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.ValidateObjectTests.json").Build();
        configuration.Bind($"ValidateObjectWithLists:{subSectionName}", dto);

        return Validator.TryValidateObject(
            dto,
            new ValidationContext(dto, serviceProvider: null, items: null),
            validationResults,
            validateAllProperties: true);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class InnerConfig
    {
#pragma warning disable SA1133
        [Required, Range(0, 5)]
#pragma warning restore SA1133
        public int SomeValue { get; set; }
        [Required(AllowEmptyStrings = false)]
        [RegularExpression(@"^[a-z]{1}$")]
        public string? OtherValue { get; set; }
    }

    private class TestConfig
    {
        [ValidateObject]
        // ReSharper disable once CollectionNeverUpdated.Local
        public List<InnerConfig>? ConfigList { get; set; }
    }

    private class NullTestConfig
    {
        [ValidateObject(SkipNullObjectValidation = true)]
        public InnerConfig? NullConfig { get; set; }
    }

    private class TestConfigWithDictionary
    {
        [ValidateObject]
        // ReSharper disable once CollectionNeverUpdated.Local
        public IDictionary<string, InnerConfig>? ConfigDict { get; set; }
    }
}