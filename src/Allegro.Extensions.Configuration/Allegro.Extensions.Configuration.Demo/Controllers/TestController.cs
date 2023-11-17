using Allegro.Extensions.Configuration.Demo.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Allegro.Extensions.Configuration.Demo.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly IOptions<TestGlobalConfig> _testGlobalConfig;

    public TestController(IOptions<TestGlobalConfig> testGlobalConfig)
    {
        _testGlobalConfig = testGlobalConfig;
    }

    [HttpGet("global")]
    public Task<TestGlobalConfig> GetGlobal()
    {
        return Task.FromResult(_testGlobalConfig.Value);
    }
}