using Azure.Identity;
using CustomPolicyApi.ApiService.DataAccess.Contract;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace CustomPolicyApi.ApiService.DataAccess
{
    public class MsGraphDataAccess : IMsGraphDataAccess
    {
        private readonly GraphServiceClient _graphClient;
        private const string MfaExtensionAttributeName = "UserMfaExtension";
        private readonly string _domain;

        public MsGraphDataAccess(IOptions<Models.OAuthOptions> authSettings)
        {
            var tenantId = authSettings.Value.MicrosoftGraph.TenantId;
            var clientId = authSettings.Value.MicrosoftGraph.ClientId;
            var clientSecret = authSettings.Value.MicrosoftGraph.ClientSecret;
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            _domain = authSettings.Value.MicrosoftGraph.Domain;

            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            _graphClient = new GraphServiceClient(clientSecretCredential, scopes);
        }

        public async Task<User?> CreateUserAsync(string email, string password)
        {
            var newUser = new User
            {
                AccountEnabled = true,
                DisplayName = email,
                MailNickname = email.Split('@')[0],
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
                        Issuer = _domain,
                        IssuerAssignedId = email
                    }
                }
            };
            var userTotpSecret = GenerateTotpSecret();

            var createdUser = await _graphClient.Users.PostAsync(newUser);
            var newExtension = new OpenTypeExtension
            {
                ExtensionName = MfaExtensionAttributeName,
                AdditionalData = new Dictionary<string, object>
                {
                    { "MfaEnabled", false },
                    { "MfaTotpSecret", userTotpSecret }
                }
            };

            await _graphClient.Users[createdUser?.Id].Extensions.PostAsync(newExtension);
            return createdUser;
        }

        public async Task DeleteUserByEmailAsync(string email)
        {
            try
            {
                var user = await GetUserByEmail(email);
                if (user != null)
                {
                    await _graphClient.Users[user.Id].DeleteAsync();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to delete user '{email}' from Graph.", ex);
            }
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            var filter = $"identities/any(id:id/issuerAssignedId eq '{email}' and id/issuer eq '{_domain}')";;

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

        public async Task EnableUserMfa(string email)
        {
            var user = await GetUserByEmail(email);
            if (user?.Id != null)
            {
                await AddOrUpdateMfaExtensionAsync(user.Id, true);
            }
        }

        public async Task DisableUserMfa(string email)
        {
            var user = await GetUserByEmail(email);
            if (user?.Id != null)
            {
                await AddOrUpdateMfaExtensionAsync(user.Id, false);
            }
        }

        public async Task<bool> GetUserMfaStatus(string email)
        {
            var user = await GetUserByEmail(email);
            if (user?.Id == null)
            {
                throw new InvalidOperationException($"User '{email}' not found.");
            }

            try
            {
                var extension = await _graphClient.Users[user.Id]
                    .Extensions[MfaExtensionAttributeName]
                    .GetAsync();

                if (extension is OpenTypeExtension openExtension &&
                    openExtension.AdditionalData != null &&
                    openExtension.AdditionalData.TryGetValue("MfaEnabled", out var mfaValue) &&
                    mfaValue is bool mfaEnabled)
                {
                    return mfaEnabled;
                }

                return false; // Default to false if extension or value is missing
            }
            catch (ODataError ex) when (ex.ResponseStatusCode == 404)
            {
                return false; // Extension not set = MFA disabled
            }
        }

        public async Task<string?> GetUserTotpSecretAsync(string email)
        {
            var user = await GetUserByEmail(email);
            if (user?.Id == null)
            {
                throw new InvalidOperationException($"User '{email}' not found.");
            }

            try
            {
                var extension = await _graphClient.Users[user.Id]
                    .Extensions[MfaExtensionAttributeName]
                    .GetAsync();

                if (extension is OpenTypeExtension openExtension &&
                    openExtension.AdditionalData != null &&
                    openExtension.AdditionalData.TryGetValue("MfaTotpSecret", out var totpSecret) &&
                    totpSecret is string secret)
                {
                    return secret;
                }

                return null; // Extension exists but no secret
            }
            catch (Microsoft.Graph.ServiceException ex) when (ex.ResponseStatusCode == 404)
            {
                return null; // Extension not found
            }
        }

        private async Task AddOrUpdateMfaExtensionAsync(string userId, bool mfaEnabled)
        {
            try
            {
                var patchExtension = new OpenTypeExtension
                {
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "MfaEnabled", mfaEnabled }
                    }
                };

                await _graphClient.Users[userId].Extensions[MfaExtensionAttributeName].PatchAsync(patchExtension);
            }
            catch (ODataError ex) when (ex.ResponseStatusCode == 404)
            {
                var newExtension = new OpenTypeExtension
                {
                    ExtensionName = MfaExtensionAttributeName,
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "MfaEnabled", mfaEnabled }
                    }
                };

                await _graphClient.Users[userId].Extensions.PostAsync(newExtension);
            }
        }
        
        private string GenerateTotpSecret(int length = 32)
        {
            // 160-bit secret by default (length = 32 base32 characters)
            byte[] secretBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(secretBytes);
            }

            return Base32Encode(secretBytes);
        }

        // RFC 4648 Base32 encoding (no padding)
        private string Base32Encode(byte[] data)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            StringBuilder result = new StringBuilder();
            int bits = 0;
            int value = 0;

            foreach (var b in data)
            {
                value = (value << 8) | b;
                bits += 8;

                while (bits >= 5)
                {
                    result.Append(alphabet[(value >> (bits - 5)) & 31]);
                    bits -= 5;
                }
            }

            if (bits > 0)
            {
                result.Append(alphabet[(value << (5 - bits)) & 31]);
            }

            return result.ToString();
        }
    }
}
