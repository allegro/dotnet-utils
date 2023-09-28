namespace Allegro.Extensions.ApplicationInsights.AspNetCore;

/// <summary>
/// Sampling modes
/// AdaptiveWithRules or FixedWithRules enable sampling ODATA exclusions rules
/// </summary>
public enum SamplingMode
{
    /// <summary>
    /// Adaptive sampling with enabled ODATA exclusion rules
    /// </summary>
    AdaptiveWithRules,

    /// <summary>
    /// Adaptive sampling without rules
    /// </summary>
    Adaptive,

    /// <summary>
    /// Fixed sampling with enabled ODATA exclusion rules
    /// </summary>
    FixedWithRules,

    /// <summary>
    /// Fixed sampling without rules
    /// </summary>
    Fixed,

    /// <summary>
    /// No sampling at all
    /// </summary>
    Disabled
}