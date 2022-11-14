using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Validators;

public static class StartupExtensions
{
    public static IServiceCollection RegisterFluentValidatorForConfig<TOptions, TValidator>(
        this IServiceCollection services,
        string? sectionName = null)
        where TOptions : class
        where TValidator : class, IValidator<TOptions>
    {
        services.ConfigureOptions<TOptions>().AddWithFluentValidation<TOptions, TValidator>(sectionName);
        return services;
    }
}