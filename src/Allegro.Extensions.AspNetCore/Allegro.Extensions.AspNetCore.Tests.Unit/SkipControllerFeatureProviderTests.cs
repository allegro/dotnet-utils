using System.Reflection;
using Allegro.Extensions.AspNetCore.Attributes;
using Allegro.Extensions.AspNetCore.Features;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace Allegro.Extensions.AspNetCore.Tests.Unit
{
    public class SkipControllerFeatureProviderTests
    {
        private static readonly IEnumerable<TypeInfo> ControllersWithoutAttribute =
            new[]
            {
                typeof(ControllerWithoutAttribute).GetTypeInfo(),
                typeof(OtherControllerWithoutAttribute).GetTypeInfo(),
            };

        private static readonly IEnumerable<TypeInfo> ControllersWithSkipOnProdAttribute =
            new[]
            {
                typeof(ControllerWithSkipOnProdAttribute).GetTypeInfo(),
                typeof(OtherControllerWithSkipOnProdAttribute).GetTypeInfo(),
            };

        [Theory]
        [InlineData("dev", "Development")]
        [InlineData("dev", "Production")]
        [InlineData("uat", "Development")]
        [InlineData("uat", "Production")]
        [InlineData("xyz", "Development")]
        public void Should_allow_test_controllers_on_test_env(string k8SEnvironment, string aspEnvironment)
        {
            // arrange
            var applicationParts = Array.Empty<ApplicationPart>();
            var controllerFeature = CreateControllerFeature(GetAllControllers);
            var sut = new SkipControllerFeatureProvider(CreateHostEnvironment(k8SEnvironment, aspEnvironment));

            // act
            sut.PopulateFeature(
                applicationParts,
                controllerFeature);

            // assert
            controllerFeature.Controllers.Should().BeEquivalentTo(GetAllControllers());
        }

        [Theory]
        [InlineData("xyz", "Production")]
        [InlineData("prod", "Production")]
        [InlineData("other", "Production")]
        public void Should_disable_test_controllers_on_non_test_env(string k8SEnvironment, string aspEnvironment)
        {
            // arrange
            var applicationParts = Array.Empty<ApplicationPart>();
            var controllerFeature = CreateControllerFeature(GetAllControllers);
            var sut = new SkipControllerFeatureProvider(CreateHostEnvironment(k8SEnvironment, aspEnvironment));

            // act
            sut.PopulateFeature(
                applicationParts,
                controllerFeature);

            // assert
            controllerFeature.Controllers.Should().BeEquivalentTo(ControllersWithoutAttribute);
        }

        [Fact]
        public void Should_allow_test_controllers_on_explicit_test_env()
        {
            // arrange
            var applicationParts = Array.Empty<ApplicationPart>();
            var controllerFeature = CreateControllerFeature(GetAllControllers);
            var sut = new SkipControllerFeatureProvider(isTestEnv: true);

            // act
            sut.PopulateFeature(
                applicationParts,
                controllerFeature);

            // assert
            controllerFeature.Controllers.Should().BeEquivalentTo(GetAllControllers());
        }

        [Fact]
        public void Should_disable_test_controllers_on_explicit_non_test_env()
        {
            // arrange
            var applicationParts = Array.Empty<ApplicationPart>();
            var controllerFeature = CreateControllerFeature(GetAllControllers);
            var sut = new SkipControllerFeatureProvider(isTestEnv: false);

            // act
            sut.PopulateFeature(
                applicationParts,
                controllerFeature);

            // assert
            controllerFeature.Controllers.Should().BeEquivalentTo(ControllersWithoutAttribute);
        }

        [Theory]
        [InlineData("vte", false)]
        [InlineData("testing", false)]
        [InlineData("dev", true)]
        [InlineData("uat", true)]
        [InlineData("xyz", true)]
        [InlineData("prod", true)]
        [InlineData("other", true)]
        public void Should_override_test_envs(string k8SEnvironment, bool shouldDisable)
        {
            // arrange
            var applicationParts = Array.Empty<ApplicationPart>();
            var controllerFeature = CreateControllerFeature(GetAllControllers);
            var sut = new SkipControllerFeatureProvider(
                CreateHostEnvironment(k8SEnvironment, "Production"),
                ["vte", "testing"]);

            // act
            sut.PopulateFeature(
                applicationParts,
                controllerFeature);

            // assert
            controllerFeature.Controllers.Should().BeEquivalentTo(
                shouldDisable ? ControllersWithoutAttribute : GetAllControllers());
        }

        private static IHostEnvironment CreateHostEnvironment(string k8SEnvironment, string aspEnvironment)
        {
            var mock = new Mock<IHostEnvironment>();
            mock.SetupProperty(x => x.EnvironmentName, aspEnvironment);
            Environment.SetEnvironmentVariable("Environment", k8SEnvironment);
            return mock.Object;
        }

        private static ControllerFeature CreateControllerFeature(Func<IEnumerable<TypeInfo>> typeInfoProvider)
        {
            var controllerFeature = new ControllerFeature();

            foreach (var typeInfo in typeInfoProvider())
            {
                controllerFeature.Controllers.Add(typeInfo);
            }

            return controllerFeature;
        }

        private static IEnumerable<TypeInfo> GetAllControllers()
        {
            return ControllersWithoutAttribute.Union(ControllersWithSkipOnProdAttribute);
        }

        private sealed class ControllerWithoutAttribute : ControllerBase
        {
        }

        private sealed class OtherControllerWithoutAttribute : ControllerBase
        {
        }

        [SkipOnProd]
        private sealed class ControllerWithSkipOnProdAttribute : ControllerBase
        {
        }

        [SkipOnProd]
        private sealed class OtherControllerWithSkipOnProdAttribute : ControllerBase
        {
        }
    }
}