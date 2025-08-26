using System.ComponentModel.DataAnnotations;
using Allegro.Extensions.Configuration;
using Allegro.Extensions.Configuration.Validation;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Vabank.Confeature.Tests.Unit;

public class ScheduledConfigurationWrapperTests
{
    [Fact]
    public void ShouldReturnDefaultValueWhenNoSchedulesAreDefined()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
#pragma warning disable CSE001
        var dto = new ValidationTestConfig();
#pragma warning restore CSE001
        configuration.Bind("ScheduledConfigWrapperConfig", dto);
        var validationResults = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(
            dto,
            new ValidationContext(dto, serviceProvider: null, items: null),
            validationResults,
            validateAllProperties: true);

        // Assert
        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
        dto.ScheduledIntegerFlag!.Value.Should().Be(1);
    }

    [Fact]
    public void ShouldReturnScheduledValueIfNoEndDateIsSpecified()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
#pragma warning disable CSE001
        var dto = new NoEndDateTestConfig();
#pragma warning restore CSE001
        configuration.Bind("ScheduledConfigWrapperConfig", dto);

        // Act & assert
        dto.ScheduledStringFlag!.Value.Should().Be("b");
    }

    private class ValidationTestConfig
    {
        [ValidateObject]
        public ScheduledConfigurationWrapper<int>? ScheduledIntegerFlag { get; set; }
    }

    private class NoEndDateTestConfig
    {
        [ValidateObject]
        public ScheduledConfigurationWrapper<string>? ScheduledStringFlag { get; set; }
    }
}