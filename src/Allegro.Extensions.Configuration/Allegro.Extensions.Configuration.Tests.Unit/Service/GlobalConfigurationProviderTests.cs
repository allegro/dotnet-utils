using Allegro.Extensions.Configuration.Api.Services;
using Allegro.Extensions.Configuration.DataContracts;
using Allegro.Extensions.Configuration.GlobalConfiguration;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Vabank.Confeature.Tests.Unit.Service;

public class GlobalConfigurationProviderTests
{
    [Fact]
    public void ShouldReturnListWithAllContextGroupsAndContextsWhenNoServiceNamePassed()
    {
        // arrange
        var sut = CreateSut(new ContextGroupsConfiguration
        {
            ContextGroups = new List<ContextGroupConfiguration>
            {
                CreateContextGroup("vabank-configuration"),
                CreateContextGroup("care-configuration"),
                CreateContextGroup("other-configuration"),
            },
        });

        // act
        var response = sut.GetGlobalConfiguration();

        // assert
        response.Should().NotBeNull();
        response.Should().BeEquivalentTo(new GetGlobalConfigurationResponse
        {
            ContextGroups = new List<ContextGroupModel>
            {
                new()
                {
                    Name = "vabank-configuration",
                    Contexts = new List<string>
                    {
                        "platform",
                        "pump",
                    },
                },
                new()
                {
                    Name = "care-configuration",
                    Contexts = new List<string>
                    {
                        "notifications",
                        "offers",
                    },
                },
                new()
                {
                    Name = "other-configuration",
                    Contexts = new List<string>
                    {
                        "other",
                    },
                },
            },
        });
    }

    [Fact]
    public void ShouldReturnEmptyListWhenNoContextGroups()
    {
        // arrange
#pragma warning disable CSE001
        var sut = CreateSut(new ContextGroupsConfiguration());
#pragma warning restore CSE001

        // act
        var response = sut.GetGlobalConfiguration();

        // assert
        response.Should().NotBeNull();
        response.ContextGroups.Should().BeEmpty();
    }

    [Fact]
    public void ShouldReturnListWithFilteredContextGroupsAndContextsWhenServiceNamePassed()
    {
        // arrange
        var sut = CreateSut(new ContextGroupsConfiguration
        {
            ContextGroups = new List<ContextGroupConfiguration>
            {
                CreateContextGroup("vabank-configuration"),
                CreateContextGroup("care-configuration"),
                CreateContextGroup("other-configuration"),
            },
        });

        // act
        var response = sut.GetGlobalConfiguration("confeaturev2");

        // assert
        response.Should().NotBeNull();
        response.Should().BeEquivalentTo(new GetGlobalConfigurationResponse
        {
            ContextGroups = new List<ContextGroupModel>
            {
                new()
                {
                    Name = "vabank-configuration",
                    Contexts = new List<string>
                    {
                        "platform",
                        "pump",
                    },
                },
                new()
                {
                    Name = "care-configuration",
                    Contexts = new List<string>
                    {
                        "offers",
                    },
                },
            },
        });
    }

    private static GlobalConfigurationProvider CreateSut(ContextGroupsConfiguration configuration) =>
        new(new OptionsWrapper<ContextGroupsConfiguration>(configuration));

    private static ContextGroupConfiguration CreateContextGroup(string name) =>
        new() { Name = name, Path = Path.Combine("Service", "test-contexts", name) };
}