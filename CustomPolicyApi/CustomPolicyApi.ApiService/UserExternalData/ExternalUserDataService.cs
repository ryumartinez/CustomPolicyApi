using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CustomPolicyApi.ApiService.UserExternalData;

public class ExternalUserDataService : IExternalUserDataService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExternalUserDataService> _logger;

    public ExternalUserDataService(IHttpClientFactory httpClientFactory, ILogger<ExternalUserDataService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<UserExternalDataResponse> GetExternalUserDataAsync(string provider, string token)
    {
        var httpClient = _httpClientFactory.CreateClient();

        _logger.LogInformation("Processing external user data request for provider: {Provider}", provider);

        return provider.ToLower() switch
        {
            "github" => await GetGitHubDataAsync(httpClient, token),
            "linkedin" => await GetLinkedInDataAsync(httpClient, token),
            "google" => await GetGoogleDataAsync(httpClient, token),
            _ => new UserExternalDataResponse("unknown", "unknown")
        };
    }

    private async Task<UserExternalDataResponse> GetGitHubDataAsync(HttpClient client, string token)
    {
        _logger.LogInformation("Sending GitHub API requests...");

        var emailRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
        var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");

        emailRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        emailRequest.Headers.UserAgent.ParseAdd("b2c-custom-policy");
        emailRequest.Headers.Accept.ParseAdd("application/vnd.github+json");

        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        userRequest.Headers.UserAgent.ParseAdd("b2c-custom-policy");
        userRequest.Headers.Accept.ParseAdd("application/vnd.github+json");

        var emailRes = await client.SendAsync(emailRequest);
        var userRes = await client.SendAsync(userRequest);

        _logger.LogInformation("GitHub email status: {Status}", emailRes.StatusCode);
        _logger.LogInformation("GitHub user status: {Status}", userRes.StatusCode);

        var emailJson = await emailRes.Content.ReadAsStringAsync();
        var userJson = await userRes.Content.ReadAsStringAsync();

        _logger.LogDebug("GitHub email response: {Json}", emailJson);
        _logger.LogDebug("GitHub user response: {Json}", userJson);

        if (!emailRes.IsSuccessStatusCode || !userRes.IsSuccessStatusCode)
            return new UserExternalDataResponse("unknown", "unknown");

        try
        {
            var emails = JsonSerializer.Deserialize<List<GitHubEmail>>(emailJson);
            var user = JsonSerializer.Deserialize<GitHubUser>(userJson);

            var fallbackEmail = emails?.FirstOrDefault(e => e.Verified) ?? emails?.FirstOrDefault();
            var email = fallbackEmail?.Email ?? "unknown";
            var avatar = user?.AvatarUrl ?? "unknown";

            _logger.LogInformation("Resolved GitHub email: {Email}", email);
            _logger.LogInformation("Resolved GitHub avatar: {Avatar}", avatar);

            return new UserExternalDataResponse(email, avatar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse GitHub responses.");
            return new UserExternalDataResponse("unknown", "unknown");
        }
    }

    private async Task<UserExternalDataResponse> GetLinkedInDataAsync(HttpClient client, string token)
    {
        _logger.LogInformation("Sending LinkedIn API requests...");

        var emailReq = new HttpRequestMessage(HttpMethod.Get,
            "https://api.linkedin.com/v2/emailAddress?q=members&projection=(elements*(handle~))");
        var profileReq = new HttpRequestMessage(HttpMethod.Get,
            "https://api.linkedin.com/v2/me?projection=(profilePicture(displayImage~:playableStreams))");

        emailReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        profileReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var emailRes = await client.SendAsync(emailReq);
        var profileRes = await client.SendAsync(profileReq);

        _logger.LogInformation("LinkedIn email status: {Status}", emailRes.StatusCode);
        _logger.LogInformation("LinkedIn profile status: {Status}", profileRes.StatusCode);

        var emailJson = await emailRes.Content.ReadAsStringAsync();
        var profileJson = await profileRes.Content.ReadAsStringAsync();

        _logger.LogDebug("LinkedIn email response: {Json}", emailJson);
        _logger.LogDebug("LinkedIn profile response: {Json}", profileJson);

        if (!emailRes.IsSuccessStatusCode || !profileRes.IsSuccessStatusCode)
            return new UserExternalDataResponse("unknown", "unknown");

        try
        {
            var emailData = JsonDocument.Parse(emailJson);
            var profileData = JsonDocument.Parse(profileJson);

            var email = emailData
                .RootElement.GetProperty("elements")[0]
                .GetProperty("handle~")
                .GetProperty("emailAddress")
                .GetString() ?? "unknown";

            var profileImage = profileData
                .RootElement.GetProperty("profilePicture")
                .GetProperty("displayImage~")
                .GetProperty("elements")[0]
                .GetProperty("identifiers")[0]
                .GetProperty("identifier")
                .GetString() ?? "unknown";

            _logger.LogInformation("Resolved LinkedIn email: {Email}", email);
            _logger.LogInformation("Resolved LinkedIn image: {Image}", profileImage);

            return new UserExternalDataResponse(email, profileImage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse LinkedIn responses.");
            return new UserExternalDataResponse("unknown", "unknown");
        }
    }

    private async Task<UserExternalDataResponse> GetGoogleDataAsync(HttpClient client, string token)
    {
        _logger.LogInformation("Sending Google API request...");

        var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request);

        _logger.LogInformation("Google profile status: {Status}", response.StatusCode);
        var profileJson = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("Google profile response: {Json}", profileJson);

        if (!response.IsSuccessStatusCode)
            return new UserExternalDataResponse("unknown", "unknown");

        try
        {
            var profile = JsonDocument.Parse(profileJson);

            var email = profile.RootElement.GetProperty("email").GetString() ?? "unknown";
            var picture = profile.RootElement.GetProperty("picture").GetString() ?? "unknown";

            _logger.LogInformation("Resolved Google email: {Email}", email);
            _logger.LogInformation("Resolved Google picture: {Picture}", picture);

            return new UserExternalDataResponse(email, picture);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Google profile.");
            return new UserExternalDataResponse("unknown", "unknown");
        }
    }
}

