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
