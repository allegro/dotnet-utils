using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegro.Extensions.Validators;

/// <summary>
/// Fluent validation extensions for options builder
/// </summary>
public static class OptionsBuilderFluentValidationExtensions
{
    /// <summary>
    /// Adds a scoped IValidateOptions&lt;TOptions&gt; service with fluent validation.
    /// </summary>
    public static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions>(
        this OptionsBuilder<TOptions> optionsBuilder) where TOptions : class
    {
        optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>>(
            provider => new FluentValidationOptions<TOptions>(
                optionsBuilder.Name, provider));
        return optionsBuilder;
    }
}