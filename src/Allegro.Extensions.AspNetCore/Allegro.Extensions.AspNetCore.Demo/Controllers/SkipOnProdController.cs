using Allegro.Extensions.AspNetCore.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Allegro.Extensions.AspNetCore.Demo.Controllers
{
    [ApiController]
    [Route("api/skip-on-prod")]
    [SkipOnProd]
    public class SkipOnProdController : ControllerBase
    {
        [HttpGet("hello")]
        public IActionResult Hello()
        {
            return Ok("Welcome to test environment! If you see this on production, something went wrong :(");
        }
    }
}