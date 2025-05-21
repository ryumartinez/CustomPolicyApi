namespace CustomPolicyApi.ApiService.DataAccess.Contract
{
    public interface IMsGraphDataAccess
    {
        Task<Microsoft.Graph.Models.User?> GetUserByEmail(string email);
        Task<Microsoft.Graph.Models.User?> CreateUserAsync(string email, string password);
    }
}
