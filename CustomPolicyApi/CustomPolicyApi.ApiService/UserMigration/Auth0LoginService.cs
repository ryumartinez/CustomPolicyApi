using System.Text;
using System.Text.Json;
using CustomPolicyApi.ApiService.Models;
using Microsoft.Extensions.Options;

namespace CustomPolicyApi.ApiService.UserMigration;

public class Auth0LoginService : IAuth0LoginService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Auth0LoginService> _logger;
    private readonly OAuthOptions _oauthOptions;

    public Auth0LoginService(
        HttpClient httpClient,
        ILogger<Auth0LoginService> logger,
        IOptions<OAuthOptions> oauthOptions)
    {
        _httpClient = httpClient;
        _logger = logger;
        _oauthOptions = oauthOptions.Value;
    }

    public async Task<Auth0LoginResult> ValidateCredentialsAsync(string email, string password)
    {
        var auth0 = _oauthOptions.Auth0;

        if (string.IsNullOrWhiteSpace(auth0.ClientId) ||
            string.IsNullOrWhiteSpace(auth0.ClientSecret) ||
            string.IsNullOrWhiteSpace(auth0.Domain))
        {
            _logger.LogError("Auth0 configuration is incomplete. Please check your environment variables or parameters.");
            return new Auth0LoginResult
            {
                IsValid = false,
                StatusCode = 500,
                Error = "Auth0 configuration is missing required values."
            };
        }

        var tokenEndpoint = $"https://{auth0.Domain}/oauth/token";

        var requestPayload = new
        {
            grant_type = "password",
            username = email,
            password = password,
            client_id = auth0.ClientId,
            client_secret = auth0.ClientSecret,
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
