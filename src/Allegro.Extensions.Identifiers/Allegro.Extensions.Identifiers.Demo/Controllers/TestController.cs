using System;
using Allegro.Extensions.Identifiers.Demo.Identifiers;
using Microsoft.AspNetCore.Mvc;

namespace Allegro.Extensions.Identifiers.Demo.Controllers
{
    [ApiController]
    [Route("srtrongly-typed")]
    public class StronglyTypedExamples : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { UserId = UserId.Generate(), PaymentId = PaymentId.Parse("123"), OrderId = OrderId.FromGuid(Guid.NewGuid()) });
        }

        [HttpPost]
        public IActionResult Post(UserId userId, PaymentId paymentId, OrderId orderId)
        {
            return Ok(new { UserId = userId, PaymentId = paymentId, OrderId = orderId });
        }
    }
}