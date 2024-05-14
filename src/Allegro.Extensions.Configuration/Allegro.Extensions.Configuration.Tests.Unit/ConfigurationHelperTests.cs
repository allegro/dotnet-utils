// using Allegro.Extensions.Configuration.Extensions;
// using Allegro.Extensions.Configuration.Models;
// using FinAi.Platform.Demo.Configuration;
//
// namespace Vabank.Confeature.Tests.Unit;
//
// using System;
// using System.Linq;
// using DeepEqual.Syntax;
// using FinAi.Platform;
// using FluentAssertions;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Configuration.Json;
// using Microsoft.Extensions.DependencyInjection;
// using Vabank.Confeature.Extensions;
// using Vabank.Confeature.Models;
// using Vabank.Confeature.Services;
// using Xunit;
//
// public class ConfigurationHelperTests
// {
//     [Fact]
//     public void TestJsonMetadata()
//     {
//         // arrange
//         var services = ConfigureServices(b => b
//             .AddJsonFile("appsettings.json")
//             .AddJsonFile("appsettings.Development.json"));
//
//         // act
//         var response = ConfigurationHelper.GetConfiguration(services);
//
//         // assert
//         response.Providers.Should().HaveCount(2);
//
//         var appsettings =
//             response.Providers.Single(x => x.Value.DisplayName == "appsettings.json");
//         var appsettingsDevelopment =
//             response.Providers.Single(x => x.Value.DisplayName == "appsettings.Development.json");
//
//         appsettings.Value.ShouldDeepEqual(new ConfigurationProviderMetadata(
//             "appsettings.json",
//             nameof(JsonConfigurationProvider),
//             "appsettings.json",
//             false,
//             true));
//         appsettingsDevelopment.Value.ShouldDeepEqual(new ConfigurationProviderMetadata(
//             "appsettings.Development.json",
//             nameof(JsonConfigurationProvider),
//             "appsettings.Development.json",
//             false,
//             true));
//     }
//
//     [Fact]
//     public void TestSecretProvider()
//     {
//         // arrange
//         var services = ConfigureServices(b => b
//             .AddJsonFile("appsettings.json")
//             .WrapSensitive()
//             .AddJsonFile("appsettings.Development.json"));
//
//         // act
//         var response = ConfigurationHelper.GetConfiguration(services);
//
//         // assert
//         response.Providers.Should().HaveCount(2);
//
//         var appsettings =
//             response.Providers.Single(x => x.Value.DisplayName == "appsettings.json");
//         var appsettingsDevelopment =
//             response.Providers.Single(x => x.Value.DisplayName == "appsettings.Development.json");
//
//         appsettings.Value.IsSecret.Should().Be(false);
//         appsettingsDevelopment.Value.IsSecret.Should().Be(true);
//
//         var appsettingsValues = response.Configuration
//             .SelectMany(x => x.Value)
//             .Where(x => x.ProviderId == appsettings.Key)
//             .ToList();
//         var appsettingsDevelopmentValues = response.Configuration
//             .SelectMany(x => x.Value)
//             .Where(x => x.ProviderId == appsettingsDevelopment.Key)
//             .ToList();
//
//         appsettingsValues.Should().NotBeEmpty();
//         appsettingsValues.ForEach(x => x.Value.Should().NotBeNull());
//
//         appsettingsDevelopmentValues.Should().NotBeEmpty();
//         appsettingsDevelopmentValues.ForEach(x => x.Value.Should().BeNull());
//     }
//
//     [Fact]
//     public void TestScheduledValueCalculation()
//     {
//         // arrange
//         var services = ConfigureServices(b => b
//             .AddJsonFile("appsettings.json"));
//
//         // act
//         var response = ConfigurationHelper.GetConfiguration(services);
//         var scheduledValue = response.Configuration
//             .FirstOrDefault(
//                 kv => kv.Key == $"{nameof(DemoConfig)}:" +
//                     $"{nameof(DemoConfig.ScheduledIntegerFlag)}:" +
//                     $"{nameof(ScheduledConfigurationWrapper<object>.Value)}").Value;
//         var configurationSchedulesConfigs = response.Configuration
//             .Where(
//                 kv => kv.Key.StartsWith(
//                     $"{nameof(DemoConfig)}:" +
//                     $"{nameof(DemoConfig.ScheduledIntegerFlag)}:" +
//                     $"{nameof(ScheduledConfigurationWrapper<object>.Schedules)}:",
//                     StringComparison.OrdinalIgnoreCase));
//
//         // assert
//         response.Providers.Should().HaveCount(1);
//
//         scheduledValue.Should().NotBeNull();
//         scheduledValue.Should().HaveCount(1);
//         scheduledValue[0].Value.Should().NotBeNull();
//         scheduledValue[0].Value.Should().Be("55");
//
//         configurationSchedulesConfigs.Should().HaveCount(0); // configuration schedules are removed from configuration keys
//     }
//
//     [Fact]
//     public void TestSecretScheduledValueCalculation()
//     {
//         // arrange
//         var services = ConfigureServices(b => b
//             .WrapSensitive().AddJsonFile("appsettings.json"));
//
//         // act
//         var response = ConfigurationHelper.GetConfiguration(services);
//         var scheduledValue = response.Configuration
//             .FirstOrDefault(
//                 kv => kv.Key == $"{nameof(DemoConfig)}:" +
//                     $"{nameof(DemoConfig.ScheduledIntegerFlag)}:" +
//                     $"{nameof(ScheduledConfigurationWrapper<object>.Value)}")
//             .Value;
//         var configurationSchedulesConfigs = response.Configuration
//             .Where(
//                 kv => kv.Key.StartsWith(
//                     $"{nameof(DemoConfig)}:" +
//                     $"{nameof(DemoConfig.ScheduledIntegerFlag)}:" +
//                     $"{nameof(ScheduledConfigurationWrapper<object>.Schedules)}:",
//                     StringComparison.OrdinalIgnoreCase));
//
//         // assert
//         response.Providers.Should().HaveCount(1);
//
//         scheduledValue.Should().NotBeNull();
//         scheduledValue.Should().HaveCount(1);
//         scheduledValue[0].Value.Should().BeNull();
//         scheduledValue[0].AdditionalInfo.Should().Be("Schedule evaluation");
//
//         // configuration schedules are removed from configuration keys
//         configurationSchedulesConfigs.Should().HaveCount(0);
//     }
//
//     private static IServiceProvider ConfigureServices(Action<ConfigurationBuilder> configure)
//     {
//         var configurationBuilder = new ConfigurationBuilder();
//         configure(configurationBuilder);
//         var configuration = configurationBuilder.Build();
//
//         var services = new ServiceCollection();
//         services.AddSingleton<IConfiguration>(configuration);
//         services.RegisterConfig<DemoConfig>(configuration, nameof(DemoConfig));
//
//         return services.BuildServiceProvider();
//     }
// }