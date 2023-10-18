using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Extensions.Options;

namespace Vabank.Confeature.Validation;

/// <summary>
/// By default Validator does not validate nested members, this validator recursively validates
/// all members of options instance (all the way down).
/// </summary>
/// <typeparam name="TOptions">Options type</typeparam>
public class DataAnnotationValidateOptionsNestedMembers<TOptions> : IValidateOptions<TOptions>
    where TOptions : class
{
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        return Validate(options);
    }

    private ValidateOptionsResult Validate(object? objectToValidate)
    {
        if (objectToValidate == null)
        {
            return ValidateOptionsResult.Skip;
        }

        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(
                objectToValidate,
                new ValidationContext(objectToValidate, serviceProvider: null, items: null),
                validationResults,
                validateAllProperties: true))
        {
            var errors = new List<string>(validationResults.Count);
            foreach (var r in validationResults)
            {
                errors.Add(
                    $"DataAnnotation validation failed for members: '{string.Join(",", r.MemberNames)}' " +
                    $"with the error: '{r.ErrorMessage}'.");
            }

            return ValidateOptionsResult.Fail(errors);
        }

        if (objectToValidate is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                var validationResult = Validate(item);
                if (validationResult.Failed)
                {
                    return validationResult;
                }
            }
        }

        var properties = objectToValidate.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(p => p.PropertyType.IsClass && !p.PropertyType.FullName!.StartsWith("System.", StringComparison.InvariantCulture));
        foreach (var property in properties)
        {
            var instance = property.GetValue(objectToValidate);

            if (instance == null)
            {
                return ValidateOptionsResult.Skip;
            }

            var validationResult = Validate(instance);
            if (validationResult.Failed)
            {
                return validationResult;
            }
        }

        return ValidateOptionsResult.Success;
    }
}