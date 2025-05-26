namespace CustomPolicyApi.ApiService.DataAccess.Contract
{
    public interface IMsGraphDataAccess
    {
        Task<Microsoft.Graph.Models.User?> GetUserByEmail(string email);
        Task<Microsoft.Graph.Models.User?> CreateUserAsync(string email, string password);
        
        Task DeleteUserByEmailAsync(string email);
        
        Task EnableUserMfa(string email);
        Task DisableUserMfa(string email);
        
        Task<bool> GetUserMfaStatus(string email);
    }
}
