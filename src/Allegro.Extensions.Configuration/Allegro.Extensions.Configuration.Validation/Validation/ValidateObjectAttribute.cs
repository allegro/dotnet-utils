using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Vabank.Confeature.Validation;

/// <summary>
/// Validates nested objects. Null objects are treated as invalid, unless they're of type that implements
/// the IEnumerable interface or SkipNullObjectValidation is set to true.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ValidateObjectAttribute : ValidationAttribute
{
    public bool SkipNullObjectValidation { get; set; }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var implementsEnumerable = ImplementsEnumerable(validationContext);

        if (value is null)
        {
            if (implementsEnumerable || SkipNullObjectValidation)
                return ValidationResult.Success;
            throw new ArgumentNullException(nameof(value));
        }

        List<ValidationResult> results = new();
        ValidationContext context = new(value, null, null);

        Validator.TryValidateObject(value, context, results, validateAllProperties: true);

        if (implementsEnumerable)
        {
            results = ValidateEnumerable(value, results);
        }

        return results.Count != 0 ? BuildCompositeResult(validationContext.DisplayName, results) : ValidationResult.Success;
    }

    private static bool ImplementsEnumerable(ValidationContext validationContext)
    {
        return validationContext
            .ObjectType
            .GetMember(
                validationContext.MemberName ??
                throw new ArgumentException($"{nameof(validationContext.MemberName)} cannot be null"))
            .GetType()
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }

    private static List<ValidationResult> ValidateEnumerable(object value, List<ValidationResult> results)
    {
        var idx = 0;

        if (value is IDictionary dictionary)
        {
            value = dictionary.Values;
        }

        foreach (var item in value as IEnumerable<object> ?? Enumerable.Empty<object>())
        {
            List<ValidationResult> innerResults = new();
            ValidationContext innerContext = new(item, null, null);

            Validator.TryValidateObject(item, innerContext, innerResults, validateAllProperties: true);
            results.AddRange(
                innerResults.Select(
                    res => new ValidationResult($"Index [{idx++}]: {res.ErrorMessage}", res.MemberNames)));
        }

        return results;
    }

    private static ValidationResult BuildCompositeResult(string contextDisplayName, List<ValidationResult> results)
    {
        StringBuilder sb = new($"{contextDisplayName} validation failed. ");
        var compositeResults = new CompositeValidationResult(string.Empty);

        results.ForEach(r =>
        {
            compositeResults.AddResult(r);
            sb.Append(r.ErrorMessage);
        });
        compositeResults.ErrorMessage = sb.ToString();

        return compositeResults;
    }
}

internal class CompositeValidationResult : ValidationResult
{
    private readonly List<ValidationResult> _results = new();

    public IEnumerable<ValidationResult> Results => _results;

    public CompositeValidationResult(string errorMessage) : base(errorMessage) { }

    public void AddResult(ValidationResult validationResult) { _results.Add(validationResult); }
}