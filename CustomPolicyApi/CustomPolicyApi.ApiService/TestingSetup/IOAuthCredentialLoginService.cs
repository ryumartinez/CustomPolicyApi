namespace CustomPolicyApi.ApiService.TestingSetup
{
    public interface IOAuthCredentialLoginService
    {
        Task<string?> GetAccessTokenAsync(string provider);
    }
}
