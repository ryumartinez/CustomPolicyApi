using CustomPolicyApi.ApiService.Models;
using CustomPolicyApi.ApiService.UserExternalData;
using Microsoft.AspNetCore.Mvc;

namespace CustomPolicyApi.ApiService.Controllers;

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
        string? token = Request.Headers["provider-token"].ToString().ToLower();
        string? provider = Request.Headers["identity-provider"].ToString().ToLower();

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(provider))
        {
            return Conflict(new { message = "Provider headers missing." });
        }
        
        var result = await _externalUserDataService.GetExternalUserDataAsync(provider, token);

        if (result.Email == "unknown")
        {
            return Conflict(new { message = "Could not retrieve external user data." });
        }
        
        return Ok(result);
    }
}