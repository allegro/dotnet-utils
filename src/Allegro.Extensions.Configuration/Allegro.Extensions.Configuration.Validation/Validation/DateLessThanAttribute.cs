using System.ComponentModel.DataAnnotations;

namespace Vabank.Confeature.Validation;

/// <summary>
/// Validation attribute used to compare DateTimeOffset a property marked with it against other DateTimeOffset property
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DateLessThanAttribute : ValidationAttribute
{
    private readonly string _comparisonProperty;

    public DateLessThanAttribute(string comparisonProperty)
    {
        _comparisonProperty = comparisonProperty;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        ErrorMessage = ErrorMessageString;

        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value is not DateTimeOffset currentValue)
        {
            throw new ArgumentException($"'{value}' is not a DateTimeOffset", nameof(value));
        }

        var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

        if (property is null)
        {
            throw new ArgumentException($"Property not found", _comparisonProperty);
        }

        if ((property.GetValue(validationContext.ObjectInstance) ?? DateTimeOffset.MaxValue) is not DateTimeOffset
            comparisonValue)
        {
            throw new ArgumentException("Property is not a DateTimeOffset", _comparisonProperty);
        }

        return currentValue > comparisonValue ? new ValidationResult(ErrorMessage) : ValidationResult.Success!;
    }

    public override bool RequiresValidationContext => true;
}