namespace CustomPolicyApi.ApiService.DataAccess.Contract
{
    public interface IMsGraphDataAccess
    {
        Task<Microsoft.Graph.Models.User> GetUserByEmail(string email);
        Task CreateUserAsync(string email, string password);
    }
}
