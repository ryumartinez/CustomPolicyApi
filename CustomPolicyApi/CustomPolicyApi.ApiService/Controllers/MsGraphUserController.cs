using CustomPolicyApi.ApiService.DataAccess.Contract;
using CustomPolicyApi.ApiService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CustomPolicyApi.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GraphUserController : ControllerBase
{
    private readonly IMsGraphDataAccess _msGraphDataAccess;
    private readonly ILogger<GraphUserController> _logger;

    public GraphUserController(
        IMsGraphDataAccess msGraphDataAccess,
        ILogger<GraphUserController> logger)
    {
        _msGraphDataAccess = msGraphDataAccess;
        _logger = logger;
    }

    [HttpGet("{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var user = await _msGraphDataAccess.GetUserByEmail(email);
        if (user is null)
        {
            return NotFound(new { message = $"User '{email}' not found in Microsoft Graph." });
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _msGraphDataAccess.CreateUserAsync(request.Email, request.Password);
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Microsoft Graph user.");
            return StatusCode(500, new { message = "Failed to create user." });
        }
    }

    [HttpDelete("{email}")]
    public async Task<IActionResult> DeleteUserByEmail(string email)
    {
        try
        {
            await _msGraphDataAccess.DeleteUserByEmailAsync(email);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Microsoft Graph user.");
            return StatusCode(500, new { message = "Failed to delete user." });
        }
    }

    [HttpPost("{email}/enable-mfa")]
    public async Task<IActionResult> EnableUserMfa(string email)
    {
        try
        {
            await _msGraphDataAccess.EnableUserMfa(email);
            return Ok(new { message = $"MFA enabled for user '{email}'." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to enable MFA for user '{email}'.");
            return StatusCode(500, new { message = $"Failed to enable MFA for user '{email}'." });
        }
    }

    [HttpPost("{email}/disable-mfa")]
    public async Task<IActionResult> DisableUserMfa(string email)
    {
        try
        {
            await _msGraphDataAccess.DisableUserMfa(email);
            return Ok(new { message = $"MFA disabled for user '{email}'." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to disable MFA for user '{email}'.");
            return StatusCode(500, new { message = $"Failed to disable MFA for user '{email}'." });
        }
    }

    [HttpGet("{email}/mfa-status")]
    public async Task<IActionResult> GetUserMfaStatus(string email)
    {
        try
        {
            var user = await _msGraphDataAccess.GetUserByEmail(email);
            if (user == null)
            {
                return NotFound(new { message = $"User '{email}' not found." });
            }

            var mfaEnabled = await _msGraphDataAccess.GetUserMfaStatus(email);
            return Ok(new { mfaEnabled });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get MFA status for user '{email}'.");
            return StatusCode(500, new { message = $"Failed to get MFA status for user '{email}'." });
        }
    }
}
