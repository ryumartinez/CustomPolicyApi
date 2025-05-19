using System.Net.Http.Headers;
using System.Text.Json;

namespace CustomPolicyApi.ApiService.UserExternalData;

public class ExternalUserDataService : IExternalUserDataService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ExternalUserDataService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<UserExternalDataResponse> GetExternalUserDataAsync(string provider, string token)
    {
        var httpClient = _httpClientFactory.CreateClient();

        switch (provider.ToLower())
        {
            case "github":
                return await GetGitHubDataAsync(httpClient, token);

            case "linkedin":
                return await GetLinkedInDataAsync(httpClient, token);

            case "google":
                return await GetGoogleDataAsync(httpClient, token);

            default:
                return new UserExternalDataResponse("unknown", "unknown");
        }
    }

    private async Task<UserExternalDataResponse> GetGitHubDataAsync(HttpClient client, string token)
    {
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

        if (!emailRes.IsSuccessStatusCode || !userRes.IsSuccessStatusCode)
            return new UserExternalDataResponse("unknown", "unknown");

        var emails = JsonSerializer.Deserialize<List<GitHubEmail>>(await emailRes.Content.ReadAsStringAsync());
        var user = JsonSerializer.Deserialize<GitHubUser>(await userRes.Content.ReadAsStringAsync());

        var fallbackEmail = emails?.FirstOrDefault(e => e.Verified) ?? emails?.FirstOrDefault();

        return new UserExternalDataResponse(fallbackEmail?.Email ?? "unknown", user?.AvatarUrl ?? "unknown");
    }

    private async Task<UserExternalDataResponse> GetLinkedInDataAsync(HttpClient client, string token)
    {
        var emailReq = new HttpRequestMessage(HttpMethod.Get,
            "https://api.linkedin.com/v2/emailAddress?q=members&projection=(elements*(handle~))");
        var profileReq = new HttpRequestMessage(HttpMethod.Get,
            "https://api.linkedin.com/v2/me?projection=(profilePicture(displayImage~:playableStreams))");

        emailReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        profileReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var emailRes = await client.SendAsync(emailReq);
        var profileRes = await client.SendAsync(profileReq);

        if (!emailRes.IsSuccessStatusCode || !profileRes.IsSuccessStatusCode)
            return new UserExternalDataResponse("unknown", "unknown");

        var emailData = JsonDocument.Parse(await emailRes.Content.ReadAsStringAsync());
        var profileData = JsonDocument.Parse(await profileRes.Content.ReadAsStringAsync());

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

        return new UserExternalDataResponse(email, profileImage);
    }

    private async Task<UserExternalDataResponse> GetGoogleDataAsync(HttpClient client, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return new UserExternalDataResponse("unknown", "unknown");

        var profile = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        var email = profile.RootElement.GetProperty("email").GetString() ?? "unknown";
        var picture = profile.RootElement.GetProperty("picture").GetString() ?? "unknown";

        return new UserExternalDataResponse(email, picture);
    }
}
