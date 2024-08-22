using Allegro.Extensions.Cqrs.Abstractions.Commands;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Cqrs.FluentValidations;

internal sealed class FluentCommandValidator<T> : ICommandValidator<T>
    where T : Command
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

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }
    }
}