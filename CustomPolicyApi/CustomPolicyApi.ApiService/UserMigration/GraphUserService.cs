using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace CustomPolicyApi.ApiService.UserMigration;

public class GraphUserService : IGraphUserService
{
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<GraphUserService> _logger;
    private readonly GraphOptions _options;

    public GraphUserService(IOptions<GraphOptions> options, ILogger<GraphUserService> logger)
    {
        _options = options.Value;
        _logger = logger;

        var credential = new ClientSecretCredential(
            _options.TenantId,
            _options.ClientId,
            _options.ClientSecret);

        _graphClient = new GraphServiceClient(credential);
    }

    public async Task<User?> CreateUserAsync(string email, string password, string displayName)
    {
        try
        {
            var user = new User
            {
                AccountEnabled = true,
                DisplayName = displayName,
                MailNickname = email.Split('@')[0],
                Identities = new List<ObjectIdentity>
                {
                    new ObjectIdentity
                    {
                        SignInType = "emailAddress",
                        Issuer = $"{_options.TenantId}.onmicrosoft.com",
                        IssuerAssignedId = email
                    }
                },
                PasswordProfile = new PasswordProfile
                {
                    Password = password,
                    ForceChangePasswordNextSignIn = false
                }
            };

            var createdUser = await _graphClient.Users.PostAsync(user);
            _logger.LogInformation("Created new user {UserPrincipalName}", createdUser.UserPrincipalName);
            return createdUser;
        }
        catch (ServiceException ex)
        {
            _logger.LogError(ex, "Microsoft Graph API error: {Message}", ex.Message);
            return null;
        }
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        try
        {
            var issuer = $"{_options.TenantId}.onmicrosoft.com";

            var filter = $"identities/any(id:id/issuerAssignedId eq '{email}' and id/issuer eq '{issuer}')";
            var result = await _graphClient.Users
                .GetAsync(config =>
                {
                    config.QueryParameters.Filter = filter;
                    config.QueryParameters.Top = 1;
                });

            var exists = result?.Value?.Any() == true;
            _logger.LogInformation("Checked existence of user {Email}: {Exists}", email, exists);
            return exists;
        }
        catch (ServiceException ex)
        {
            _logger.LogError(ex, "Error checking user existence in Graph: {Message}", ex.Message);
            return false;
        }
    }
}