using Allegro.Extensions.Cqrs.Abstractions.Commands;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Cqrs.FluentValidations;

//TODO: add validator for Data Annotations as separate package
//TODO: we can add option or mention in readme that validation on asp .net pipeline can be disabled: https://www.talkingdotnet.com/disable-automatic-model-state-validation-in-asp-net-core-2-1/
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

        //TODO: what about validation context?
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