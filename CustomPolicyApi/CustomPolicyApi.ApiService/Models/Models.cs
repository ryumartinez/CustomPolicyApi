using System.Text.Json.Serialization;

namespace CustomPolicyApi.ApiService.Models;

public class Auth0User
{
    public string UserId { get; set; } = "";
    public string Email { get; set; } = "";
    public bool EmailVerified { get; set; }
    public string? Name { get; set; }
}

public class PreLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class Auth0LoginResult
{
    public bool IsValid { get; set; }
    public int StatusCode { get; set; }
    public string? Error { get; set; }
    public string? RawResponse { get; set; }
}

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

public class CreateUserRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class ValidateCredentialsRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
