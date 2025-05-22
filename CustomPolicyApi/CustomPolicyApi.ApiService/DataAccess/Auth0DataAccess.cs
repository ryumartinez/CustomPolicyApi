using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using CustomPolicyApi.ApiService.DataAccess.Contract;
using CustomPolicyApi.ApiService.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace CustomPolicyApi.ApiService.DataAccess
{
    public class Auth0DataAccess : IAuth0DataAccess
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<Auth0DataAccess> _logger;
        private readonly Models.OAuthOptions _oauthOptions;
        private readonly ManagementApiClient _managementApiClient;

        public Auth0DataAccess(HttpClient httpClient, ILogger<Auth0DataAccess> logger, IOptions<Models.OAuthOptions> authSettings)
        {
            var managementToken = authSettings.Value.Auth0.ManagementToken;
            var uriIdentifier = new Uri(authSettings.Value.Auth0.UriIdentifier);
            _managementApiClient = new ManagementApiClient(managementToken, uriIdentifier);
            _httpClient = httpClient;
            _logger = logger;
            _oauthOptions = authSettings.Value;
        }


        public async Task CreateUserAsync(string email, string password)
        {
            var userCreateRequest = new UserCreateRequest
            {
                Email = email,
                Password = password,
                Connection = "Username-Password-Authentication", // required: your DB connection name
                EmailVerified = false,
                AppMetadata = new Dictionary<string, object>     // optional
        {
            { "role", "user" }
        }
            };

            var createdUser = await _managementApiClient.Users.CreateAsync(userCreateRequest);

            // You can return or log the user if needed
            Console.WriteLine($"Created user with ID: {createdUser.UserId}");
        }

        public async Task<Auth0.ManagementApi.Models.User?> GetUserByEmailAsync(string email)
        {
            var user = await _managementApiClient.Users.GetUsersByEmailAsync(email);
            return user.First();
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
}
