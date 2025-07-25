using Allegro.Extensions.Cqrs.Abstractions;
using Allegro.Extensions.Cqrs.Abstractions.Queries;
using Allegro.Extensions.Cqrs.Demo.Commands;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Allegro.Extensions.Cqrs.Demo.Queries;

public record BarData(string SomeData);
internal sealed record BarQuery(string? SomeId) : Query<BarData>;

internal sealed class BarQueryFluentValidator : AbstractValidator<BarQuery>
{
    public BarQueryFluentValidator()
    {
        RuleFor(p => p.SomeId).NotEmpty();
    }
}

internal sealed class BarQueryValidator : IQueryValidator<BarQuery>
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

internal sealed class BarQueryHandler : IQueryHandler<BarQuery, BarData>
{
    public Task<BarData> Handle(BarQuery query, CancellationToken cancellationToken)
    {
        // should take data directly from read model on dedicates sql query to view of some data
        return Task.FromResult(new BarData("Some data 1"));
    }
}

[Decorator]
internal sealed class BarQueryHandlerDecorator : IQueryHandler<BarQuery, BarData>
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
#pragma warning disable CA1848
        _logger.LogInformation("Before handle");
        var result = await _decorated.Handle(query, cancellationToken);
        _logger.LogInformation("After handle");
#pragma warning restore CA1848
        return result;
    }
}