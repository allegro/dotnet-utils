using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Allegro.Extensions.Configuration.Validation;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Allegro.Extensions.Configuration.Tests;

/// <summary>
/// Global configuration correctness tests base class. To be used inside repositories with global contexts.
/// </summary>
public abstract class GlobalConfigurationCorrectnessFixture : GlobalConfigurationFixtureBase
{
    protected GlobalConfigurationCorrectnessFixture(ITestOutputHelper output, string repoRootPath)
        : base(output, repoRootPath)
    {
    }

    protected void Validate()
    {
        ValidateDataAnnotations();
        ValidateServicesOrder();
    }

    protected void CheckForValidationAttributesPresence()
    {
        foreach (var configurationType in ConfigurationTypes)
        {
            CheckInternal(configurationType);
        }
    }

    private void ValidateDataAnnotations()
    {
        foreach (var type in ConfigurationTypes)
        {
            Output.WriteLine($"Currently testing: {type}");

#pragma warning disable CA2201
            var cfg = Activator.CreateInstance(type) ?? throw new Exception($"Could not create type: {type}");
#pragma warning restore CA2201

            var attribute = type.GetCustomAttribute<GlobalConfigurationContextAttribute>();
            Configuration.Bind(attribute!.SectionName, cfg);
            var ctx = new ValidationContext(cfg!);
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(cfg!, ctx, results, validateAllProperties: true))
            {
                var sb = new StringBuilder();
                foreach (var validationResult in results)
                {
                    sb.Append(validationResult.ErrorMessage);
                    sb.Append(", ");
                    Output.WriteLine(validationResult.ErrorMessage);
                }

                throw new XunitException(sb.ToString());
            }
        }
    }

    private void ValidateServicesOrder()
    {
        foreach (var configurationFile in ConfigurationFiles)
        {
            try
            {
                using var fileStream = new FileStream(
                    configurationFile.FullName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite);
                using var jsonDocument = JsonDocument.Parse(fileStream);
                if (!jsonDocument.RootElement.TryGetProperty("metadata", out var metadataElement) ||
                    !metadataElement.TryGetProperty("services", out var servicesElement))
                {
#pragma warning disable CA2201
                    throw new Exception(
                        $"[{configurationFile.Name}]: Configuration file should contain .metadata.services list.");
#pragma warning restore CA2201
                }

                var services = servicesElement
                    .EnumerateArray()
                    .Select(
                        x => x.GetString() ??
#pragma warning disable CA2201
                             throw new Exception($"[{configurationFile.Name}]: Service name cannot be null."))
#pragma warning restore CA2201
                    .ToList();

                for (var i = 1; i < services.Count; i++)
                {
                    if (StringComparer.OrdinalIgnoreCase.Compare(services[i], services[i - 1]) < 0)
                    {
#pragma warning disable CA2201
                        throw new Exception(
                            $"[{configurationFile.Name}]: Invalid services order. '{services[i - 1]}' precedes '{services[i]}'.");
#pragma warning restore CA2201
                    }
                }
            }
            catch (Exception e)
            {
#pragma warning disable CA2201
                throw new Exception($"[{configurationFile.Name}]: Error reading configuration file: {e.Message}", e);
#pragma warning restore CA2201
            }
        }
    }

    private static void CheckInternal(Type configurationType)
    {
        var properties = configurationType.GetProperties();
        foreach (var propertyInfo in properties)
        {
            // do not validate properties that are read-only or marked with NoValidationOnDeploy
            if (!propertyInfo.CanWrite ||
                propertyInfo.GetCustomAttribute<NoValidationOnDeployAttribute>() is not null)
            {
                continue;
            }

            // property is not valid if it has 0 validation attributes and is not marked with the [NoValidationOnDeploy]
            if (!propertyInfo.GetCustomAttributes<ValidationAttribute>().Any())
                throw new XunitException(
                    $"Type: {configurationType} has no attributes defined for property: {propertyInfo}. " +
                    $"If this property should not be validated during configuration deployment, use the {nameof(NoValidationOnDeployAttribute)}");

            // property is of user-defined type - we need to validate it recursively
            if (propertyInfo.PropertyType.IsClass &&
                propertyInfo.PropertyType.Assembly.FullName == configurationType.Assembly.FullName)
            {
                CheckInternal(propertyInfo.PropertyType);
            }
        }
    }
}