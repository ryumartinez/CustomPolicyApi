using Microsoft.AspNetCore.Mvc;

namespace CustomPolicyApi.ApiService.UserMigration;

[ApiController]
[Route("api/validate-prelogin")]
public class PreLoginValidationController : ControllerBase
{
    private readonly ILogger<PreLoginValidationController> _logger;
    private readonly IAuth0LoginService _auth0LoginService;

    public PreLoginValidationController(
        ILogger<PreLoginValidationController> logger,
        IAuth0LoginService auth0LoginService)
    {
        _logger = logger;
        _auth0LoginService = auth0LoginService;
    }

    [HttpPost]
    public async Task<IActionResult> Validate([FromBody] PreLoginRequest request)
    {
        _logger.LogInformation("Pre-login validation started for: {Email}", request.SignInName);

        if (string.IsNullOrWhiteSpace(request.SignInName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                version = "1.0.0",
                status = 400,
                action = "ValidationError",
                userMessage = "Email and password are required."
            });
        }

        var result = await _auth0LoginService.ValidateCredentialsAsync(request.SignInName, request.Password);

        if (!result.IsValid)
        {
            _logger.LogWarning("Auth0 validation failed for {Email} with status {StatusCode}. Error: {Error}",
                request.SignInName, result.StatusCode, result.Error);

            return BadRequest(new
            {
                version = "1.0.0",
                status = 400,
                action = "ValidationError",
                userMessage = "Invalid email or password."
            });
        }

        _logger.LogInformation("Auth0 validation succeeded for {Email}", request.SignInName);

        return Ok(new
        {
            version = "1.0.0",
            status = 200,
            action = "Continue"
        });
    }
}