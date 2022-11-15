using System.Threading;
using System.Threading.Tasks;
using Allegro.Extensions.Cqrs.Abstractions;
using Allegro.Extensions.Cqrs.Abstractions.Queries;
using Allegro.Extensions.Cqrs.Demo.Commands;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Allegro.Extensions.Cqrs.Demo.Queries;

public record BarData(string SomeData);
internal record BarQuery(string? SomeId) : IQuery<BarData>;

internal class BarQueryFluentValidator : AbstractValidator<BarQuery>
{
    public BarQueryFluentValidator()
    {
        RuleFor(p => p.SomeId).NotEmpty();
    }
}

internal class BarQueryValidator : IQueryValidator<BarQuery>
{
    public Task Validate(BarQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(query.SomeId))
        {
            throw new ValidationException("Missing some id!");
        }

        return Task.CompletedTask;
    }
}

internal class BarQueryHandler : IQueryHandler<BarQuery, BarData>
{
    public Task<BarData> Handle(BarQuery query, CancellationToken cancellationToken)
    {
        // should take data directly from read model on dedicates sql query to view of some data
        return Task.FromResult<BarData>(new BarData("Some data 1"));
    }
}

[Decorator]
internal class BarQueryHandlerDecorator : IQueryHandler<BarQuery, BarData>
{
    private readonly IQueryHandler<BarQuery, BarData> _decorated;
    private readonly ILogger<BarCommand> _logger;

    public BarQueryHandlerDecorator(IQueryHandler<BarQuery, BarData> decorated, ILogger<BarCommand> logger)
    {
        _decorated = decorated;
        _logger = logger;
    }

    public async Task<BarData> Handle(BarQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Before handle");
        var result = await _decorated.Handle(query, cancellationToken);
        _logger.LogInformation("After handle");
        return result;
    }
}