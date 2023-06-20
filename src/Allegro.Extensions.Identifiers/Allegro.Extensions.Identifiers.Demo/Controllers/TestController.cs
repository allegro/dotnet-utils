using Allegro.Extensions.Identifiers.Demo.Identifiers;
using Microsoft.AspNetCore.Mvc;

namespace Allegro.Extensions.Identifiers.Demo.Controllers
{
    [ApiController]
    [Route("strongly-typed")]
    public class StronglyTypedExamples : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { UserId = UserId.Generate(), PaymentId = PaymentId.Parse("123"), OrderId = OrderId.FromGuid(Guid.NewGuid()) });
        }

        [HttpPost]
        public IActionResult Post([FromQuery] UserId userId, [FromQuery] PaymentId paymentId, [FromQuery] OrderId orderId)
        {
            return Ok(new { UserId = userId, PaymentId = paymentId, OrderId = orderId });
        }

        [HttpGet("{userId}")]
        public IActionResult Post([FromRoute] UserId userId)
        {
            return Ok(new { UserId = userId });
        }

        [HttpPost("body")]
        public IActionResult Post([FromBody] Dto dto)
        {
            return Ok(dto);
        }

        public record Dto(UserId UserId, PaymentId PaymentId, OrderId OrderId);
    }
}