using System.ComponentModel.DataAnnotations;
using Vabank.Confeature.Validation;

// ReSharper disable once CheckNamespace
namespace Vabank.Confeature;

/// <summary>
/// Wrapper used to schedule configuration changes
/// </summary>
/// <remarks>Use the <see cref="Vabank.Confeature.Validation.ValidateObjectAttribute"/> attribute on fields
/// of ScheduledConfigurationWrapper type</remarks>
/// <typeparam name="T">Wrapped configuration value type</typeparam>
[Serializable]
public class ScheduledConfigurationWrapper<T>
{
    /// <summary>
    /// Fallback used when current date is not in range of any schedule
    /// </summary>
    [Required]
    public T? DefaultValue { get; set; }

    /// <summary>
    /// Configuration schedules containing date ranges and associated value.
    /// </summary>
    [Required]
    [DateOverlapValidation(
        nameof(ConfigurationSchedule<T>.StartDate),
        nameof(ConfigurationSchedule<T>.EndDate),
        ErrorMessage = "Scheduled dates cannot overlap")]
    public ConfigurationSchedule<T>[] Schedules { get; set; } = Array.Empty<ConfigurationSchedule<T>>();

    /// <summary>
    /// Current calculated value
    /// </summary>
    public T? Value
    {
        get
        {
            var now = DateTimeOffset.UtcNow;

            foreach (var schedule in Schedules.OrderByDescending(c => c.StartDate))
            {
                if (now >= schedule.StartDate && (!schedule.EndDate.HasValue || now < schedule.EndDate))
                {
                    return schedule.ScheduledValue;
                }
            }

            return DefaultValue;
        }
    }
}