namespace CustomPolicyApi.ApiService.UserMigration;

public class Auth0User
{
    public string UserId { get; set; } = "";
    public string Email { get; set; } = "";
    public bool EmailVerified { get; set; }
    public string? Name { get; set; }
}