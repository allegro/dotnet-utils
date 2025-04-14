using Allegro.Extensions.Configuration.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Allegro.Extensions.Configuration.Api.Controllers
{
    [ApiController]
    [Route("global-configuration")]
    public class GlobalConfigurationController : ControllerBase
    {
        private readonly IGlobalConfigurationProvider _globalConfigurationProvider;

        public GlobalConfigurationController(IGlobalConfigurationProvider globalConfigurationProvider)
        {
            _globalConfigurationProvider = globalConfigurationProvider;
        }

        [HttpGet("context-groups")]
        public IActionResult GetGlobalConfiguration([FromQuery] string? serviceName)
        {
            return Ok(_globalConfigurationProvider.GetGlobalConfiguration(serviceName));
        }

        [HttpGet("context-groups/{contextGroupName}/contexts/{contextName}")]
        public IActionResult GetGlobalConfigurationContext(
            [FromRoute] string contextGroupName,
            [FromRoute] string contextName)
        {
            return File(
                _globalConfigurationProvider.GetContext(contextGroupName, contextName),
                "application/json",
                contextName);
        }
    }
}