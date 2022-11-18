using Allegro.Extensions.Cqrs.Abstractions.Commands;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Cqrs.FluentValidations;

internal class FluentCommandValidator<T> : ICommandValidator<T>
    where T : ICommand
{
    private readonly IServiceProvider _serviceProvider;

    public FluentCommandValidator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Validate(T command)
    {
        var requestValidators = _serviceProvider.GetServices<IValidator<T>>();

        var failures = (await Task.WhenAll(requestValidators.Select(v => v.ValidateAsync(command))))
           .SelectMany(result => result.Errors)
           .Where(f => f != null)
           .ToList();

        if (failures.Any())
        {
            throw new ValidationException(failures);
        }
    }
}