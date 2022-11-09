using System;
using System.Collections.Generic;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegro.Extensions.Validators;

public class FluentValidationOptions<TOptions>
    : IValidateOptions<TOptions> where TOptions : class
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string? _name;
    public FluentValidationOptions(string? name, IServiceProvider serviceProvider)
    {
        // we need the service provider to create a scope later
        _serviceProvider = serviceProvider;
        _name = name; // Handle named options
    }

    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        // Null name is used to configure all named options.
        if (_name != null && _name != name)
        {
            // Ignored if not validating this instance.
            return ValidateOptionsResult.Skip;
        }

        // Ensure options are provided to validate against
        ArgumentNullException.ThrowIfNull(options);

        // Validators are typically registered as scoped,
        // so we need to create a scope to be safe, as this
        // method is be called from the root scope
        using var scope = _serviceProvider.CreateScope();

        // retrieve an instance of the validator
        var validator = scope.ServiceProvider.GetRequiredService<IValidator<TOptions>>();

        // Run the validation
        var results = validator.Validate(options);
        if (results.IsValid)
        {
            // All good!
            return ValidateOptionsResult.Success;
        }

        // Validation failed, so build the error message
        var typeName = options.GetType().Name;
        var errors = new List<string>();
        foreach (var result in results.Errors)
        {
            errors.Add($"Fluent validation failed for '{typeName}.{result.PropertyName}' with the error: '{result.ErrorMessage}'.");
        }

        return ValidateOptionsResult.Fail(errors);
    }
}