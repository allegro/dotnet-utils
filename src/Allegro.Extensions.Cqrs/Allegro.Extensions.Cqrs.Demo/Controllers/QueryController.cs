using Allegro.Extensions.Cqrs.Abstractions.Queries;
using Allegro.Extensions.Cqrs.Demo.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Allegro.Extensions.Cqrs.Demo.Controllers;

[ApiController]
[Route("queries")]
public class QueryController : ControllerBase
{
    private readonly IQueryDispatcher _queryDispatcher;

    public QueryController(IQueryDispatcher queryDispatcher)
    {
        _queryDispatcher = queryDispatcher;
    }

    [HttpGet("bar")]
    public async Task<IActionResult> Bar([FromQuery] string? id, CancellationToken cancellationToken)
    {
        var result = await _queryDispatcher.Query(new BarQuery(id), cancellationToken);
        return Ok(result);
    }
}