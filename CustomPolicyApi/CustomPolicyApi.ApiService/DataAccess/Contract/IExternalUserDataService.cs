using CustomPolicyApi.ApiService.Models;

namespace CustomPolicyApi.ApiService.UserExternalData;

public interface IExternalUserDataService
{
    Task<UserExternalDataResponse> GetExternalUserDataAsync(string provider, string token);
}
