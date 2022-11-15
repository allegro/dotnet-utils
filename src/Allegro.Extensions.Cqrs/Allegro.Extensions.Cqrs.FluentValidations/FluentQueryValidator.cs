using Allegro.Extensions.Cqrs.Abstractions.Queries;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Cqrs.FluentValidations;

internal class FluentQueryValidator<T> : IQueryValidator<T>
    where T : IQuery
{
    private readonly IServiceProvider _serviceProvider;

    public FluentQueryValidator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Validate(T command, CancellationToken cancellationToken)
    {
        //TODO: multiple validators
        var requestValidators = _serviceProvider.GetServices<IValidator<T>>();

        var failures = (await Task.WhenAll(requestValidators.Select(v => v.ValidateAsync(command, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            throw new ValidationException(failures);
        }
    }
}