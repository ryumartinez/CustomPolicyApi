using CustomPolicyApi.ApiService.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CustomPolicyApi.ApiService.TestingSetup
{
    public class OAuthCredentialLoginService : IOAuthCredentialLoginService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OAuthOptions _oauthOptions;
        private readonly OAuthTestCredentials _testCreds;

        public OAuthCredentialLoginService(
            IHttpClientFactory httpClientFactory,
            IOptions<OAuthOptions> oauthOptions,
            IOptions<OAuthTestCredentials> testCreds)
        {
            _httpClientFactory = httpClientFactory;
            _oauthOptions = oauthOptions.Value;
            _testCreds = testCreds.Value;
        }

        public async Task<string?> GetAccessTokenAsync(string provider)
        {
            if (string.IsNullOrWhiteSpace(_testCreds.Email) || string.IsNullOrWhiteSpace(_testCreds.Password))
                throw new InvalidOperationException("Missing test email or password from configuration.");

            var email = _testCreds.Email;
            var password = _testCreds.Password;
            var client = _httpClientFactory.CreateClient();

            var payload = provider.ToLowerInvariant() switch
            {
                "google" => new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["username"] = email,
                    ["password"] = password,
                    ["client_id"] = _oauthOptions.Google.ClientId,
                    ["client_secret"] = _oauthOptions.Google.ClientSecret,
                    ["scope"] = "openid email"
                },
                "linkedin" => new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["username"] = email,
                    ["password"] = password,
                    ["client_id"] = _oauthOptions.LinkedIn.ClientId,
                    ["client_secret"] = _oauthOptions.LinkedIn.ClientSecret
                },
                "auth0" => new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["username"] = email,
                    ["password"] = password,
                    ["client_id"] = _oauthOptions.Auth0.ClientId,
                    ["client_secret"] = _oauthOptions.Auth0.ClientSecret,
                    ["scope"] = "openid"
                },
                _ => throw new NotSupportedException($"Unsupported provider: {provider}")
            };

            var tokenUrl = provider.ToLowerInvariant() switch
            {
                "google" => "https://oauth2.googleapis.com/token",
                "linkedin" => "https://www.linkedin.com/oauth/v2/accessToken",
                "auth0" => $"https://{_oauthOptions.Auth0.Domain}/oauth/token",
                _ => throw new NotSupportedException($"Unsupported provider: {provider}")
            };

            var response = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(payload));
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Token request failed for {provider}: {response.StatusCode} - {content}");
            }

            using var json = JsonDocument.Parse(content);
            return json.RootElement.GetProperty("access_token").GetString();
        }
    }
}
