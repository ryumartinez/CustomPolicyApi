namespace CustomPolicyApi.ApiService.UserMigration;

public interface IAuth0UserLookupService
{
    Task<bool> UserExistsAsync(string email);
}