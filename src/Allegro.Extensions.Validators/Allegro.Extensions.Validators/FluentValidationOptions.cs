/* The MIT License (MIT)

Copyright (c) 2016 andrewlock

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */
using System;
using System.Collections.Generic;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegro.Extensions.Validators;

/// <summary>
/// FluentValidation options extension.
/// </summary>
public class FluentValidationOptions<TOptions> : IValidateOptions<TOptions>
    where TOptions : class
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string? _name;

    /// <param name="name">Option name.</param>
    /// <param name="serviceProvider">Service provider for scoped option handling.</param>
    public FluentValidationOptions(string? name, IServiceProvider serviceProvider)
    {
        // we need the service provider to create a scope later
        _serviceProvider = serviceProvider;
        _name = name; // Handle named options
    }

    /// <summary>
    /// Validate TOptions using a registered IValidator&lt;TOptions&gt;
    /// </summary>
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        // Null name is used to configure all named options.
        if (_name != null && _name != name)
        {
            return ValidateOptionsResult.Skip;
        }

        ArgumentNullException.ThrowIfNull(options);

        using var scope = _serviceProvider.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IValidator<TOptions>>();

        var results = validator.Validate(options);
        if (results.IsValid)
        {
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