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

public class LinkedInEmailResponse
{
    [JsonPropertyName("elements")]
    public List<LinkedInEmailElement> Elements { get; set; } = new();
}

public class LinkedInEmailElement
{
    [JsonPropertyName("handle~")]
    public LinkedInEmailHandle Handle { get; set; } = new();
}

public class LinkedInEmailHandle
{
    [JsonPropertyName("emailAddress")]
    public string EmailAddress { get; set; } = "";
}

public class LinkedInProfileResponse
{
    [JsonPropertyName("profilePicture")]
    public LinkedInProfilePicture ProfilePicture { get; set; } = new();
}

public class LinkedInProfilePicture
{
    [JsonPropertyName("displayImage~")]
    public LinkedInDisplayImage DisplayImage { get; set; } = new();
}

public class LinkedInDisplayImage
{
    [JsonPropertyName("elements")]
    public List<LinkedInImageElement> Elements { get; set; } = new();
}

public class LinkedInImageElement
{
    [JsonPropertyName("identifiers")]
    public List<LinkedInImageIdentifier> Identifiers { get; set; } = new();
}

public class LinkedInImageIdentifier
{
    [JsonPropertyName("identifier")]
    public string Identifier { get; set; } = "";
}

public class GoogleProfileResponse
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("picture")]
    public string Picture { get; set; } = "";
}
