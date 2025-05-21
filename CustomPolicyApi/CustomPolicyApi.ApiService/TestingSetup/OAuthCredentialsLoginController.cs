using Microsoft.AspNetCore.Mvc;

namespace CustomPolicyApi.ApiService.TestingSetup
{
    [ApiController]
    [Route("api/oauth-credentials")]
    public class OAuthCredentialLoginController : ControllerBase
    {
        private readonly IOAuthCredentialLoginService _service;
        private readonly ILogger<OAuthCredentialLoginController> _logger;

        public OAuthCredentialLoginController(
            IOAuthCredentialLoginService service,
            ILogger<OAuthCredentialLoginController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves an OAuth access token for the specified provider using shared test credentials.
        /// </summary>
        /// <param name="provider">Provider name: google, linkedin, or auth0.</param>
        [HttpGet("{provider}")]
        public async Task<IActionResult> GetAccessToken([FromRoute] string provider)
        {
            try
            {
                var token = await _service.GetAccessTokenAsync(provider);
                return Ok(new
                {
                    provider,
                    access_token = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token retrieval failed for provider {Provider}", provider);
                return BadRequest(new
                {
                    error = ex.Message,
                    provider
                });
            }
        }
    }
}
