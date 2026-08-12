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
    private readonly IOptions<TestLocalConfig> _testLocalConfig;

    public TestController(
        IOptions<TestGlobalConfig> testGlobalConfig,
        IOptions<TestLocalConfig> testLocalConfig)
    {
        _testGlobalConfig = testGlobalConfig;
        _testLocalConfig = testLocalConfig;
    }

    [HttpGet("global")]
    public Task<TestGlobalConfig> GetGlobal()
    {
        return Task.FromResult(_testGlobalConfig.Value);
    }

    [HttpGet("local")]
    public Task<TestLocalConfig> GetLocal()
    {
        return Task.FromResult(_testLocalConfig.Value);
    }
}