using Azure.Identity;
using CustomPolicyApi.ApiService.DataAccess.Contract;
using CustomPolicyApi.ApiService.Models;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using System.Net;

namespace CustomPolicyApi.ApiService.DataAccess
{
    public class MsGraphDataAccess : IMsGraphDataAccess
    {
        private readonly GraphServiceClient _graphClient;

        public MsGraphDataAccess(IOptions<Models.OAuthOptions> authSettings)
        {
            var tenantId = authSettings.Value.MicrosoftGraph.TenantId;
            var clientId = authSettings.Value.MicrosoftGraph.ClientId;
            var clientSecret = authSettings.Value.MicrosoftGraph.ClientSecret;
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            _graphClient = new GraphServiceClient(clientSecretCredential, scopes);
        }

        public async Task CreateUserAsync(string email, string password)
        {
            var newUser = new User
            {
                AccountEnabled = true,
                DisplayName = email,
                MailNickname = email.Split('@')[0],
                UserPrincipalName = email,
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = false,
                    Password = password
                },
                Identities = new List<ObjectIdentity>
            {
                new ObjectIdentity
                {
                    SignInType = "emailAddress",
                    Issuer = "<your-b2c-tenant-name>.onmicrosoft.com", // e.g., contoso.onmicrosoft.com
                    IssuerAssignedId = email
                }
            }
            };

            return await _graphClient.Users.PostAsync(newUser);
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            var filter = $"identities/any(id:id/issuerAssignedId eq '{email}' and id/issuer eq '<your-b2c-tenant-name>.onmicrosoft.com')";

            try
            {
                var users = await _graphClient.Users
                    .GetAsync(request => request.QueryParameters.Filter = filter);

                return users?.Value?.FirstOrDefault();
            }
            catch (ODataError ex) when (ex.ResponseStatusCode == (int)HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
