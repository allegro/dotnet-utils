using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Vabank.Confeature.Validation;

/// <summary>
/// Validation attribute used to validate whether date ranges in enumerable do not overlap
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DateOverlapValidationAttribute : ValidationAttribute
{
    private readonly string _startDatePropertyName;
    private readonly string _endDatePropertyName;

    public DateOverlapValidationAttribute(string startDateProperty, string endDateProperty)
    {
        _startDatePropertyName = startDateProperty;
        _endDatePropertyName = endDateProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        ErrorMessage = ErrorMessageString;

        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value is not IEnumerable enumerable)
        {
            throw new ArgumentException($"'{value}' is not an IEnumerable", nameof(value));
        }

        var elementType = enumerable.GetType().GetElementType();
        if (elementType is null)
        {
            throw new ArgumentNullException(nameof(value), $"Could not infer element type for '{value} enumerable'");
        }

        var startDateProperty = elementType.GetProperty(_startDatePropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (startDateProperty is null)
        {
            throw new ArgumentNullException(_startDatePropertyName, $"Could not find start date property {_startDatePropertyName} in {elementType}");
        }

        var endDateProperty = elementType.GetProperty(_endDatePropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (endDateProperty is null)
        {
            throw new ArgumentNullException(_endDatePropertyName, $"Could not find end date property {_endDatePropertyName} in {elementType}");
        }

        var dateRanges = new List<(DateTimeOffset StartDate, DateTimeOffset EndDate)>();

        foreach (var item in enumerable)
        {
            if ((startDateProperty.GetValue(item) ?? DateTimeOffset.MinValue) is not DateTimeOffset startDate)
            {
                throw new ArgumentException($"{_startDatePropertyName} is not a DateTimeOffset", _startDatePropertyName);
            }

            if ((endDateProperty.GetValue(item) ?? DateTimeOffset.MaxValue) is not DateTimeOffset endDate)
            {
                throw new ArgumentException($"{_endDatePropertyName} is not a DateTimeOffset", _endDatePropertyName);
            }

            dateRanges.Add((startDate, endDate));
        }

        dateRanges = dateRanges.OrderBy(d => d.StartDate).ToList();

        for (var i = 0; i < dateRanges.Count - 1; i++)
        {
            if (dateRanges[i].StartDate < dateRanges[i + 1].EndDate &&
                dateRanges[i + 1].StartDate < dateRanges[i].EndDate)
            {
                return new ValidationResult(ErrorMessage);
            }
        }

        return ValidationResult.Success;
    }
}