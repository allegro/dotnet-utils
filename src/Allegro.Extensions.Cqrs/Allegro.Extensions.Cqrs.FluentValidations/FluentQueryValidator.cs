using Allegro.Extensions.Cqrs.Abstractions.Queries;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Allegro.Extensions.Cqrs.FluentValidations;

internal class FluentQueryValidator<T> : IQueryValidator<T>
    where T : Query
{
    private readonly IServiceProvider _serviceProvider;

    public FluentQueryValidator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Validate(T query, CancellationToken cancellationToken)
    {
        var requestValidators = _serviceProvider.GetServices<IValidator<T>>();

        var failures = (await Task.WhenAll(requestValidators.Select(v => v.ValidateAsync(query, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            throw new ValidationException(failures);
        }
    }
}