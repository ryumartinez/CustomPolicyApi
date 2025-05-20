namespace CustomPolicyApi.ApiService.UserMigration;

public class Auth0User
{
    public string UserId { get; set; } = "";
    public string Email { get; set; } = "";
    public bool EmailVerified { get; set; }
    public string? Name { get; set; }
}

public class Auth0Options
{
    public string Domain { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}

public class GraphOptions
{
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
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
