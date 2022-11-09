using System.Threading;
using System.Threading.Tasks;
using Allegro.Extensions.Cqrs.Abstractions.Queries;
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

    [HttpGet]
    public async Task<IActionResult> Execute([FromQuery] string id, CancellationToken cancellationToken)
    {
        var result = await _queryDispatcher.Query(new SampleQuery(id), cancellationToken);
        return Ok(result);
    }
}

internal record SampleQuery(string SomeId) : IQuery<SampleData>;

public record SampleData(string SomeData);

internal class SampleQueryHandler : IQueryHandler<SampleQuery, SampleData>
{
    public Task<SampleData?> Handle(SampleQuery query, CancellationToken cancellationToken)
    {
        // should take data directly from read model on dedicates sql query to view of some data
        return Task.FromResult<SampleData?>(new SampleData("Some data 1"));
    }
}