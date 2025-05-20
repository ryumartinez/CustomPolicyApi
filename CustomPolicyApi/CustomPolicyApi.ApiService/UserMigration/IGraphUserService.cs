using Microsoft.Graph.Models;

namespace CustomPolicyApi.ApiService.UserMigration;

public interface IGraphUserService
{
    Task<User?> CreateUserAsync(string email, string password, string displayName);
}