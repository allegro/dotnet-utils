using Allegro.Extensions.Configuration.Configuration;
using Allegro.Extensions.Configuration.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegro.Extensions.Configuration.Services;

internal static class OptionsRegistrationValidator
{
    internal static void Validate(IServiceProvider serviceProvider)
    {
        if (IsDisabled())
            return;

        var optionsType = typeof(IOptions<>);
        var postConfigureOptionsType = typeof(IPostConfigureOptions<>);
        var configureOptionsType = typeof(IConfigureOptions<>);

        var options = serviceProvider.GetRequiredService<IOptions<RegistrationValidatorOptions>>().Value;

        var constructorParameterTypes = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Where(
                a => a.FullName is not null &&
                     options.AssemblyPrefixesToValidate.Any(p =>
                         a.FullName.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            .SelectMany(a => a.GetExportedTypes())
            .SelectMany(
                type => type
                    .GetConstructors()
                    .SelectMany(ctor => ctor.GetParameters().Select(paramInfo => paramInfo.ParameterType)),
                (ctorType, paramType) => new CtorWithParam(ctorType, paramType));

        var optionValueTypes = constructorParameterTypes
            .Where(item => IsAssignableToGenericType(item.ParamType, optionsType))
            .Select(item => item with { ParamType = item.ParamType.GetGenericArguments().Single() })
            // check only options marked with the Confeature interface
            .Where(item => item.ParamType.GetInterface(nameof(IConfigurationMarker)) is not null)
            .Distinct();

        var scope = serviceProvider.CreateScope();
        var postConfiguration = optionValueTypes
            // For now we only validate simple types as it's rather cumbersome to make complex generic config types
            .Where(item => !item.ParamType.IsGenericType && !item.ParamType.ContainsGenericParameters)
            .Select(
                item => new
                {
                    item.CtorType,
                    OptionsType = item.ParamType,
                    ConfigureTypes = scope.ServiceProvider.GetServices(configureOptionsType.MakeGenericType(item.ParamType)),
                    PostConfigureTypes = scope.ServiceProvider.GetServices(postConfigureOptionsType.MakeGenericType(item.ParamType))
                });

        var emptyConfigurations = postConfiguration
            .Where(item => item.ConfigureTypes.Any() == false && item.PostConfigureTypes.Any() == false)
            .Select(item => new CtorWithParam(item.CtorType, item.OptionsType))
            .Where(item => item.ParamType != typeof(EnvironmentConfiguration))
            // Exclude any *optional* Platform configuration, e.g. AccessTokenRefreshHealthCheckOptions, MetricsOptions
            .Where(type => options.NamespacesToIgnore.All(ns => type.ParamType.Namespace?.StartsWith(ns, StringComparison.OrdinalIgnoreCase) == false));

        if (emptyConfigurations.Any())
        {
            throw new OptionsNotRegisteredException(emptyConfigurations);
        }
    }

    // stackoverflow.com/questions/74616/how-to-detect-if-type-is-another-generic-type/1075059#1075059
    private static bool IsAssignableToGenericType(Type givenType, Type genericType)
    {
        var interfaceTypes = givenType.GetInterfaces();
        if (interfaceTypes.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType))
            return true;

        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            return true;

        var baseType = givenType.BaseType;
        if (baseType == null)
            return false;

        return IsAssignableToGenericType(baseType, genericType);
    }

    private static bool IsDisabled()
    {
        const string envVarName = "DisableOptionsRegistrationValidation";
        var isDisabledStr = Environment.GetEnvironmentVariable(envVarName) ??
                            bool.FalseString;

        if (!bool.TryParse(isDisabledStr, out var isDisabled))
        {
            Console.WriteLine($"Could not parse the environment variable {envVarName} to a boolean");
            return true;
        }

        Console.WriteLine($"OptionsRegistrationValidation is currently {(isDisabled ? "disabled" : "enabled")}");
        return isDisabled;
    }

    private class OptionsNotRegisteredException : Exception
    {
        public OptionsNotRegisteredException(IEnumerable<CtorWithParam> types)
            : base($"Following configuration types were not registered properly:{Environment.NewLine}" +
                   $"{string.Join($", {Environment.NewLine}", types)}.{Environment.NewLine}" +
                   "Please use the RegisterConfig<T> method for config DTOs registration. " +
                   $"If you think this is a bug, please contact the Aard team on the #help-aard Slack channel " +
                   $"or disable the {nameof(OptionsRegistrationValidator)} by setting the " +
                   $"DisableOptionsRegistrationValidation environment variable to true.")
        {
        }
    }

    private readonly record struct CtorWithParam(Type CtorType, Type ParamType);
}