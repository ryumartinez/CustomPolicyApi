using CustomPolicyApi.ApiService.DataAccess.Contract;
using CustomPolicyApi.ApiService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CustomPolicyApi.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Auth0UserController : ControllerBase
{
    private readonly IAuth0DataAccess _auth0DataAccess;
    private readonly ILogger<Auth0UserController> _logger;

    public Auth0UserController(
        IAuth0DataAccess auth0DataAccess,
        ILogger<Auth0UserController> logger)
    {
        _auth0DataAccess = auth0DataAccess;
        _logger = logger;
    }

    [HttpGet("{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var user = await _auth0DataAccess.GetUserByEmailAsync(email);
        if (user is null)
        {
            return NotFound(new { message = $"User '{email}' not found in Auth0." });
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _auth0DataAccess.CreateUserAsync(request.Email, request.Password);
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Auth0 user.");
            return StatusCode(500, new { message = "Failed to create user." });
        }
    }

    [HttpDelete("{email}")]
    public async Task<IActionResult> DeleteUserByEmail(string email)
    {
        try
        {
            await _auth0DataAccess.DeleteUserByEmailAsync(email);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Auth0 user.");
            return StatusCode(500, new { message = "Failed to delete user." });
        }
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateCredentials([FromBody] ValidateCredentialsRequest request)
    {
        var result = await _auth0DataAccess.ValidateCredentialsAsync(request.Email, request.Password);
        if (result.IsValid)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
}
