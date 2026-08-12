using System.ComponentModel.DataAnnotations;
using Allegro.Extensions.Configuration.Validation;

namespace Allegro.Extensions.Configuration;

/// <summary>
/// Used to determine configuration schedules
/// </summary>
/// <typeparam name="T">Wrapped configuration value type</typeparam>
public class ConfigurationSchedule<T>
{
    /// <summary>
    /// Value to be used between <see cref="StartDate"/> and <see cref="EndDate"/>
    /// </summary>
    [Required]
    public T? ScheduledValue { get; init; }

    /// <summary>
    /// Start date of range. Has to be less than <see cref="EndDate"/>
    /// </summary>
    [Required]
    [DateLessThan(nameof(EndDate), ErrorMessage = "StartDate must be before EndDate")]
    public DateTimeOffset StartDate { get; init; }

    /// <summary>
    /// End date of range. If <see langword="null"/>, then <code>DateTimeOffset.MaxValue</code> is used
    /// </summary>
    public DateTimeOffset? EndDate { get; init; }
}