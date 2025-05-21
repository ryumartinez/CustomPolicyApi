using CustomPolicyApi.ApiService.Models;

namespace CustomPolicyApi.ApiService.DataAccess.Contract
{
    public interface IAuth0DataAccess
    {
        Task<Auth0.ManagementApi.Models.User?> GetUserByEmailAsync(string email);
        Task CreateUserAsync(string email, string password);
        Task<Auth0LoginResult> ValidateCredentialsAsync(string email, string password);
    }
}
