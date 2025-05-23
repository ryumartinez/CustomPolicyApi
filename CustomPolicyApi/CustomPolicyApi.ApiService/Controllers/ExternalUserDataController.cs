using CustomPolicyApi.ApiService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CustomPolicyApi.ApiService.UserExternalData;

[ApiController]
[Route("api/[controller]")]
public class ExternalUserDataController : ControllerBase
{
    private readonly IExternalUserDataService _externalUserDataService;

    public ExternalUserDataController(IExternalUserDataService externalUserDataService)
    {
        _externalUserDataService = externalUserDataService;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserExternalDataResponse), 200)]
    [ProducesResponseType(typeof(UserExternalDataResponse), 400)]
    public async Task<IActionResult> GetExternalUserData()
    {
        string? authHeader = Request.Headers["provider-token"].ToString().ToLower();
        string? provider = Request.Headers["identity-provider"].ToString().ToLower();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ") ||
            string.IsNullOrEmpty(provider))
        {
            return BadRequest(new UserExternalDataResponse("unknown", "unknown"));
        }

        string token = authHeader.Substring("Bearer ".Length);
        var result = await _externalUserDataService.GetExternalUserDataAsync(provider, token);

        if (result.Email == "unknown")
            return BadRequest(result);

        return Ok(result);
    }
}