using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using Allegro.Extensions.Cqrs.Abstractions.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Allegro.Extensions.Cqrs.Demo.Controllers;

[ApiController]
[Route("commands")]
public class CommandController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;

    public CommandController(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    [HttpPost]
    public async Task<IActionResult> Execute(SampleCommand command)
    {
        await _commandDispatcher.Send(command);
        return Ok(command.Id);
    }
}

public record SampleCommand(string Name) : ICommand
{
    public string Id { get; } = Guid.NewGuid().ToString();
}

internal class SampleCommandHandler : ICommandHandler<SampleCommand>
{
    private readonly ILogger<SampleCommandHandler> _logger;

    public SampleCommandHandler(ILogger<SampleCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SampleCommand command)
    {
        _logger.LogInformation("Handle");
        // do some work, change state, send side effects, threat as unit of work
        return Task.CompletedTask;
    }
}

internal class SampleCommandValidator : ICommandValidator<SampleCommand>
{
    public Task Validate(SampleCommand command)
    {
        if (command.Name == "Not valid super-truper based on some externals sources validation :D")
        {
            throw new ValidationException("Not valid name. More sophisticated validations here");
        }

        return Task.CompletedTask;
    }
}

internal class SampleCommandActions : ICommandExecutionActions<SampleCommand>
{
    private readonly ILogger<SampleCommandActions> _logger;
    private readonly Stopwatch _timer;

    public SampleCommandActions(ILogger<SampleCommandActions> logger)
    {
        _logger = logger;
        _timer = new Stopwatch();
    }

    public Task Before(SampleCommand command)
    {
        // example some diagnostics, metrics, unit of work (transaction start and commit) or else
        _timer.Start();
        _logger.LogInformation("Before executed");
        return Task.CompletedTask;
    }

    public Task After(SampleCommand command)
    {
        _timer.Stop();
        _logger.LogInformation($"After executed in {_timer.Elapsed}");
        return Task.CompletedTask;
    }
}