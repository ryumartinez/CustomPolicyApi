using Microsoft.AspNetCore.Mvc;

namespace CustomPolicyApi.ApiService.UserMigration;

[ApiController]
[Route("api/validate-prelogin")]
public class PreLoginValidationController : ControllerBase
{
    private readonly ILogger<PreLoginValidationController> _logger;

    public PreLoginValidationController(ILogger<PreLoginValidationController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult Validate([FromBody] PreLoginRequest request)
    {
        _logger.LogInformation("Pre-login validation for: {Email}", request.SignInName);

        // 🔐 Never log passwords or persist them!
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

        // 🧠 Custom logic to block users, check reputation, etc.
        if (request.SignInName.EndsWith("@banned.com", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                version = "1.0.0",
                status = 400,
                action = "ValidationError",
                userMessage = "This email is blocked from signing in."
            });
        }

        // ✅ Allow B2C to continue to login-NonInteractive
        return Ok(new
        {
            version = "1.0.0",
            status = 200,
            action = "Continue"
        });
    }
}