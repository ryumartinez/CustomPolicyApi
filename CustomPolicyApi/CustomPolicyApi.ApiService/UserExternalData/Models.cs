namespace CustomPolicyApi.ApiService.UserExternalData;

public record UserExternalDataResponse(string Email, string ProfileImage);

public class GitHubEmail
{
    public string Email { get; set; } = string.Empty;
    public bool Verified { get; set; }
}

public class GitHubUser
{
    public string AvatarUrl { get; set; } = string.Empty;
}
