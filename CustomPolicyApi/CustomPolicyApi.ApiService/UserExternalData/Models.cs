using System.Text.Json.Serialization;

namespace CustomPolicyApi.ApiService.UserExternalData;

public record UserExternalDataResponse(string Email, string ProfileImage);

public class GitHubEmail
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    [JsonPropertyName("primary")]
    public bool Primary { get; set; }
}

public class GitHubUser
{
    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; } = "";
}

