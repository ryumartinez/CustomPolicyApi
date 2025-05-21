using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using CustomPolicyApi.ApiService.DataAccess.Contract;
using CustomPolicyApi.ApiService.Models;
using Microsoft.Extensions.Options;
using static Azure.Core.HttpHeader;

namespace CustomPolicyApi.ApiService.DataAccess
{
    public class Auth0DataAccess : IAuth0DataAccess
    {
        private readonly ManagementApiClient _managementApiClient;

        public Auth0DataAccess(IOptions<OAuthOptions> authSettings)
        {
            var managementToken = authSettings.Value.Auth0.ManagementToken;
            var uriIdentifier = new Uri(authSettings.Value.Auth0.UriIdentifier);
            _managementApiClient = new ManagementApiClient(managementToken, uriIdentifier);
        }


        public async Task CreateUserAsync(string email, string password)
        {
            var userCreateRequest = new UserCreateRequest
            {
                Email = email,
                Password = password,
                Connection = "Username-Password-Authentication", // required: your DB connection name
                EmailVerified = false,
                AppMetadata = new Dictionary<string, object>     // optional
        {
            { "role", "user" }
        }
            };

            var createdUser = await _managementApiClient.Users.CreateAsync(userCreateRequest);

            // You can return or log the user if needed
            Console.WriteLine($"Created user with ID: {createdUser.UserId}");
        }

        public async Task<Auth0.ManagementApi.Models.User?> GetUserByEmailAsync(string email)
        {
            var user = await _managementApiClient.Users.GetUsersByEmailAsync(email);
            return user.First();
        }
    }
}
