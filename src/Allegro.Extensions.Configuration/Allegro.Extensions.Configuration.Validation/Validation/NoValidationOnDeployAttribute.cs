namespace Allegro.Extensions.Configuration.Validation;

/// <summary>
/// Marker attribute to explicitly indicate fields and properties that should not be validated
/// during configuration deployment. It does not influence the validation on service startup.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class NoValidationOnDeployAttribute : Attribute
{
}

/// <summary>
/// Marker attribute to explicitly indicate fields and properties that should not be validated.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
[Obsolete($"Please use {nameof(NoValidationOnDeployAttribute)} instead")]
public class NoValidationAttribute : NoValidationOnDeployAttribute
{
}