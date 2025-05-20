using System.Text;
using System.Text.Json;

namespace CustomPolicyApi.ApiService.UserMigration;

//Migration steps
//1-The custom policy checks if the user exists in ad b2c
//2-If the user exists the flow continue as it is
//3-If the user does not exist the custom policy calls a REST endpoint
//4-The REST endpoint checks against Auth0 to see if the user exists there
//5-If it does not exist the user gets an error telling that they must create an account
//6-If it exists in Auth0, the backend creates a new user with the same email and password using Graph

// Current user journey login flow
// 1 - The SelfAsserted-LocalAccountSignin-Email technical profile displays the login form
// 2 - The user enters their email and password
// 3 - On form submission, B2C captures the input claims (signInName and password)
// 4 - The ValidationTechnicalProfile (login-NonInteractive) is invoked to authenticate the user
// 5 - login-NonInteractive uses the Resource Owner Password Credentials (ROPC) grant
//     to request an ID token from the B2C identity provider
// 6 - If the credentials are valid, the identity provider returns an ID token
// 7 - The output claims (e.g., objectId, givenName, upn) from the ID token are issued to the B2C engine
// 8 - The UseTechnicalProfileForSessionManagement issues a session cookie (via SM-AAD)
// 9 - The user is redirected to the next step in the orchestration or the final redirect URI

public class Auth0LoginService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Auth0LoginService> _logger;

    public Auth0LoginService(HttpClient httpClient, ILogger<Auth0LoginService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> LoginAsync(string domain, string clientId, string clientSecret, string audience, string username, string password)
    {
        var url = $"https://{domain}/oauth/token";

        var payload = new
        {
            grant_type = "password",
            username,
            password,
            audience,
            client_id = clientId,
            client_secret = clientSecret,
            scope = "openid profile email"
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);

        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Auth0 login failed: {StatusCode} - {Response}", response.StatusCode, responseJson);
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var token = doc.RootElement.GetProperty("access_token").GetString();
            _logger.LogInformation("Successfully obtained Auth0 token.");
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Auth0 token response.");
            return null;
        }
    }
}