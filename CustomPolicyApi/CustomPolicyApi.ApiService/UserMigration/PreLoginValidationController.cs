using Microsoft.AspNetCore.Mvc;

namespace CustomPolicyApi.ApiService.UserMigration;

[ApiController]
[Route("api/[controller]")]
public class PreLoginValidationController : ControllerBase
{
    private readonly IGraphUserService _graphUserService;
    private readonly IAuth0UserLookupService _auth0UserLookupService;
    private readonly IAuth0LoginService _auth0LoginService;
    private readonly ILogger<PreLoginValidationController> _logger;

    public PreLoginValidationController(
        IGraphUserService graphUserService,
        IAuth0UserLookupService auth0UserLookupService,
        IAuth0LoginService auth0LoginService,
        ILogger<PreLoginValidationController> logger)
    {
        _graphUserService = graphUserService;
        _auth0UserLookupService = auth0UserLookupService;
        _auth0LoginService = auth0LoginService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> ValidateUser([FromBody] PreLoginRequest request)
    {
        _logger.LogInformation("Validating pre-login for email: {Email}", request.Email);

        // Step 1: Check if user exists in Azure AD B2C (Graph)
        var userExistsInAzure = await _graphUserService.UserExistsAsync(request.Email);
        if (userExistsInAzure)
        {
            _logger.LogInformation("User already exists in Azure. Continuing policy flow.");
            return Ok(); // Proceed with policy flow
        }

        // Step 2: Check if user exists in Auth0
        var userExistsInAuth0 = await _auth0UserLookupService.UserExistsAsync(request.Email);
        if (!userExistsInAuth0)
        {
            _logger.LogWarning("User does not exist in either Azure or Auth0: {Email}", request.Email);
            return BadRequest(new { message = "User does not exist." });
        }

        // Step 3: Validate credentials in Auth0
        var authResult = await _auth0LoginService.ValidateCredentialsAsync(request.Email, request.Password);
        if (authResult.StatusCode != 200)
        {
            _logger.LogWarning("Auth0 password validation failed: {Message}", authResult.Error ?? "Unknown error");
            return BadRequest(new { message = "Your password is incorrect." });
        }

        // Step 4: Create user in Azure using Graph API
        var user = await _graphUserService.CreateUserAsync(request.Email, request.Password, request.Email);
        if (user == null)
        {
            _logger.LogError("Failed to create user in Azure AD B2C.");
            return StatusCode(500, new { message = "An error occurred while creating the user." });
        }

        _logger.LogInformation("User successfully migrated to Azure: {Email}", request.Email);
        return Ok();
    }
}