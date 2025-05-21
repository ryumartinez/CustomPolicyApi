using System.Net.Http.Headers;
using System.Text.Json;
using CustomPolicyApi.ApiService.Models;
using Microsoft.Extensions.Options;

namespace CustomPolicyApi.ApiService.UserMigration;

public class Auth0UserLookupService : IAuth0UserLookupService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Auth0UserLookupService> _logger;
    private readonly Auth0Options _auth0;
    private string? _managementToken;

    public Auth0UserLookupService(
        HttpClient httpClient,
        ILogger<Auth0UserLookupService> logger,
        IOptions<OAuthOptions> oauthOptions)
    {
        _httpClient = httpClient;
        _logger = logger;
        _auth0 = oauthOptions.Value.Auth0;
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        var token = await GetManagementApiTokenAsync();
        if (token == null)
        {
            _logger.LogError("Failed to obtain Auth0 management API token.");
            return false;
        }

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://{_auth0.Domain}/api/v2/users-by-email?email={Uri.EscapeDataString(email)}");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to query user: {StatusCode} - {Body}", response.StatusCode, json);
            return false;
        }

        try
        {
            var users = JsonSerializer.Deserialize<List<Auth0User>>(json);
            return users?.Any() == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse users-by-email response.");
            return false;
        }
    }

    private async Task<string?> GetManagementApiTokenAsync()
    {
        if (_managementToken != null)
            return _managementToken;

        var payload = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _auth0.ClientId,
            ["client_secret"] = _auth0.ClientSecret,
            ["audience"] = $"https://{_auth0.Domain}/api/v2/"
        };

        var response = await _httpClient.PostAsync(
            $"https://{_auth0.Domain}/oauth/token",
            new FormUrlEncodedContent(payload));

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Auth0 token request failed: {Status} - {Content}", response.StatusCode, content);
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            _managementToken = doc.RootElement.GetProperty("access_token").GetString();
            return _managementToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract access_token.");
            return null;
        }
    }
}
