using CustomPolicyApi.ApiService.Models;
using Microsoft.Graph.Models;

namespace CustomPolicyApi.ApiService.DataAccess.Contract
{
    public interface IAuth0DataAccess
    {
        Task<Auth0.ManagementApi.Models.User?> GetUserByEmailAsync(string email);
        Task<Auth0.ManagementApi.Models.User?> CreateUserAsync(string email, string password);
        Task<Auth0LoginResult> ValidateCredentialsAsync(string email, string password);
        
        Task DeleteUserByEmailAsync(string email);
    }
}
