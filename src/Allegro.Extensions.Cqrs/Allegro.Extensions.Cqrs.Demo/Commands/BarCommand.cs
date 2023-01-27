using System.Threading.Tasks;
using Allegro.Extensions.Cqrs.Abstractions;
using Allegro.Extensions.Cqrs.Abstractions.Commands;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Allegro.Extensions.Cqrs.Demo.Commands;

public record BarCommand(string Name) : Command;

internal class BarCommandFluentValidator : AbstractValidator<BarCommand>
{
    public BarCommandFluentValidator()
    {
        RuleFor(p => p.Name).NotEmpty();
    }
}

internal class BarCommandValidator : ICommandValidator<BarCommand>
{
    public Task Validate(BarCommand command)
    {
        if (string.IsNullOrEmpty(command.Name))
        {
            throw new ValidationException("Missing name!");
        }

        return Task.CompletedTask;
    }
}

internal class BarCommandHandler : ICommandHandler<BarCommand>
{
    private readonly ILogger<BarCommandHandler> _logger;

    public BarCommandHandler(ILogger<BarCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(BarCommand command)
    {
        _logger.LogInformation("Handle Bar");
        return Task.CompletedTask;
    }
}

[Decorator]
internal class BarCommandHandlerDecorator : ICommandHandler<BarCommand>
{
    private readonly ICommandHandler<BarCommand> _decorated;
    private readonly ILogger<BarCommand> _logger;

    public BarCommandHandlerDecorator(ICommandHandler<BarCommand> decorated, ILogger<BarCommand> logger)
    {
        _decorated = decorated;
        _logger = logger;
    }

    public async Task Handle(BarCommand command)
    {
        _logger.LogInformation("Before handle");
        await _decorated.Handle(command);
        _logger.LogInformation("After handle");
    }
}