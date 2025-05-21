using CustomPolicyApi.ApiService.DataAccess.Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomPolicyApi.ApiService.UserMigration;

[ApiController]
[Route("api/[controller]")]
public class PreLoginValidationController : ControllerBase
{
    private readonly IMsGraphDataAccess _msGraphDataAccess;
    private readonly IAuth0DataAccess _auth0DataAccess;
    private readonly ILogger<PreLoginValidationController> _logger;

    public PreLoginValidationController(
        IMsGraphDataAccess msGraphDataAccess,
        IAuth0DataAccess auth0DataAccess,
        ILogger<PreLoginValidationController> logger)
    {
        _msGraphDataAccess = msGraphDataAccess;
        _auth0DataAccess = auth0DataAccess;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> ValidateUser([FromBody] PreLoginRequest request)
    {
        _logger.LogInformation("Validating pre-login for email: {Email}", request.Email);

        // Step 1: Check if user exists in Azure AD B2C
        var userInGraph = await _msGraphDataAccess.GetUserByEmail(request.Email);
        if (userInGraph is not null)
        {
            _logger.LogInformation("User already exists in Azure. Continuing policy flow.");
            return Ok(); // Proceed with policy flow
        }

        // Step 2: Check if user exists in Auth0
        var userInAuth0 = await _auth0DataAccess.GetUserByEmailAsync(request.Email);
        if (userInAuth0 is null)
        {
            _logger.LogWarning("User does not exist in either Azure or Auth0: {Email}", request.Email);
            return BadRequest(new { message = "User does not exist." });
        }

        // Step 3: Validate credentials in Auth0
        var authResult = await _auth0DataAccess.ValidateCredentialsAsync(request.Email, request.Password);
        if (authResult.StatusCode != 200)
        {
            _logger.LogWarning("Auth0 password validation failed: {Message}", authResult.Error ?? "Unknown error");
            return BadRequest(new { message = "Your password is incorrect." });
        }

        // Step 4: Create user in Azure
        var createdUser = await _msGraphDataAccess.CreateUserAsync(request.Email, request.Password);
        if (createdUser is null)
        {
            _logger.LogError("Failed to create user in Azure AD B2C.");
            return StatusCode(500, new { message = "An error occurred while creating the user." });
        }

        _logger.LogInformation("User successfully migrated to Azure: {Email}", request.Email);
        return Ok();
    }
}
