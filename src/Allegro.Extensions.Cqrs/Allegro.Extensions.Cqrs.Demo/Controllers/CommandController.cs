using Allegro.Extensions.Cqrs.Abstractions.Commands;
using Allegro.Extensions.Cqrs.Demo.Commands;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost("bar")]
    public async Task<IActionResult> Bar(BarCommand command)
    {
        await _commandDispatcher.Send(command);
        return Ok(command.Id);
    }
}