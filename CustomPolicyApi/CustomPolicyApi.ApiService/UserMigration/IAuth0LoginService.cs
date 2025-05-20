namespace CustomPolicyApi.ApiService.UserMigration;

public interface IAuth0LoginService
{
    Task<bool> ValidateCredentialsAsync(string email, string password);
}