using Allegro.Extensions.AspNetCore.Attributes;
using Allegro.Extensions.Configuration.Api.Services;
using Allegro.Extensions.Configuration.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Allegro.Extensions.Configuration.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("secrets")]
    [SkipOnProd]
    public class SecretsController : ControllerBase
    {
        private readonly ISecretsProvider _secretsProvider;

        public SecretsController(ISecretsProvider secretsProvider)
        {
            _secretsProvider = secretsProvider;
        }

        [HttpPost]
        public IActionResult GetSecrets([FromBody] GetSecretsRequest request)
        {
            return Content(
                _secretsProvider.GetSecretsAsJson(request.KeyVaultPrefixes),
                "application/json");
        }
    }
}