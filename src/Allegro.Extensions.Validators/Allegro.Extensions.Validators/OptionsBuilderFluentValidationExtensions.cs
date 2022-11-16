using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegro.Extensions.Validators;

public static class OptionsBuilderFluentValidationExtensions
{
    /// <summary>
    /// Adds a scoped IValidateOptions&lt;TOptions&gt; service with fluent validation.
    /// </summary>
    public static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions>(
        this OptionsBuilder<TOptions> optionsBuilder) where TOptions : class
    {
        optionsBuilder.Services.AddScoped<IValidateOptions<TOptions>>(
            provider => new FluentValidationOptions<TOptions>(
                optionsBuilder.Name, provider));
        return optionsBuilder;
    }
}