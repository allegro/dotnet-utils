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
using System.Collections.Generic;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegro.Extensions.Validators;

/// <summary>
/// Startup extensions for FluentValidation
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    /// Register all IValidator&lt;T&gt; fluent validators in specified assemblies
    /// </summary>
    public static IServiceCollection RegisterFluentValidators(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies)
    {
        return services.Scan(s => s.FromAssemblies(assemblies)
            .AddClasses(c => c.AssignableTo(typeof(IValidator<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
    }

    /// <summary>
    /// Adds a new IOptions&lt;TOptions&gt; to the service collection.
    /// </summary>
    public static OptionsBuilder<TOptions> AddOptionsWithFluentValidation<TOptions, TValidator>(
        this IServiceCollection services,
        string? configurationSection)
        where TOptions : class
        where TValidator : class, IValidator<TOptions>
    {
        // Add the validator
        services.AddScoped<IValidator<TOptions>, TValidator>();

        return services.AddOptions<TOptions>()
            .BindConfiguration(configurationSection)
            .ValidateFluentValidation()
            .ValidateOnStart();
    }
}