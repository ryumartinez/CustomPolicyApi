namespace CustomPolicyApi.ApiService.UserMigration;

public interface IAuth0LoginService
{
    Task<Auth0LoginResult> ValidateCredentialsAsync(string email, string password);
}