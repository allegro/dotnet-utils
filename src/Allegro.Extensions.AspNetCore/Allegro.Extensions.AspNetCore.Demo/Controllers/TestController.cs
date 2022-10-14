using Microsoft.AspNetCore.Mvc;

namespace Allegro.Extensions.AspNetCore.Demo.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        [HttpGet("hello")]
        public IActionResult Hello()
        {
            return Ok("Welcome to production!");
        }
    }
}