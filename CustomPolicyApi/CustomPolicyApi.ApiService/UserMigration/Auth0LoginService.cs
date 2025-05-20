using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

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
//     to request an ID token from the B2C identity provider (POST to https://login.microsoftonline.com/{tenant}/oauth2/token)
// 6 - If the credentials are invalid, the token endpoint returns a 400 error 
//     {"error": "invalid_grant", "error_description": "AADB2C90117: The user does not exist."}
// 7 - The error is caught by the SelfAsserted technical profile and mapped to a localized string (e.g., UserMessageIfUserDoesNotExist)
//     If defined in LocalizedResources, the friendly error message is shown to the user on the same screen
// 8 - If the credentials are valid, the identity provider returns an ID token with user claims
// 9 - The output claims (e.g., objectId, givenName, upn) are extracted and returned to the policy engine
// 10 - The UseTechnicalProfileForSessionManagement issues a session token (SSO cookie via SM-AAD)
// 11 - The user is redirected to the next orchestration step or the final redirect URI (e.g., back to the app)


public class Auth0LoginService : IAuth0LoginService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Auth0LoginService> _logger;
    private readonly Auth0Options _options;

    public Auth0LoginService(
        HttpClient httpClient,
        ILogger<Auth0LoginService> logger,
        IOptions<Auth0Options> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<Auth0LoginResult> ValidateCredentialsAsync(string email, string password)
    {
        var tokenEndpoint = $"https://{_options.Domain}/oauth/token";

        var requestPayload = new
        {
            grant_type = "password",
            username = email,
            password = password,
            audience = _options.Audience,
            client_id = _options.ClientId,
            client_secret = _options.ClientSecret,
            scope = "openid"
        };

        var json = JsonSerializer.Serialize(requestPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(tokenEndpoint, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Auth0 credentials valid for user {Email}", email);
                return new Auth0LoginResult
                {
                    IsValid = true,
                    StatusCode = (int)response.StatusCode,
                    RawResponse = responseBody
                };
            }

            _logger.LogWarning("Auth0 login failed for {Email}: {Error}", email, responseBody);
            return new Auth0LoginResult
            {
                IsValid = false,
                StatusCode = (int)response.StatusCode,
                Error = responseBody,
                RawResponse = responseBody
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Auth0 token endpoint");
            return new Auth0LoginResult
            {
                IsValid = false,
                StatusCode = 500,
                Error = ex.Message
            };
        }
    }
}