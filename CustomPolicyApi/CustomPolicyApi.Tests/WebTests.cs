using System.Text;
using System.Text.Json;

namespace CustomPolicyApi.Tests;

public class WebTests
{
    [TestCase("google")]
    [TestCase("linkedin")]
    [TestCase("auth0")]
    public async Task GetAccessTokenFromCredentialLoginControllerReturnsValidToken(string provider)
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.CustomPolicyApi_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync();
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("apiservice");

        await resourceNotificationService
            .WaitForResourceAsync("apiservice", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(30));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/oauth-credentials/{provider}");

        // Act
        var response = await httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Failed to retrieve token for provider '{provider}': {content}");

        using var json = JsonDocument.Parse(content);
        var root = json.RootElement;

        Assert.That(root.TryGetProperty("access_token", out var tokenProp), Is.True, $"Missing access_token in response: {content}");
        Assert.That(tokenProp.GetString(), Is.Not.Null.And.Not.Empty, "access_token was empty or null");
    }

    [TestCase("github", "GITHUB_TEST_TOKEN")]
    [TestCase("google", "GOOGLE_TEST_TOKEN")]
    [TestCase("linkedin", "LINKEDIN_TEST_TOKEN")]
    public async Task GetExternalUserDataWithTokenReturnsOkStatusCode(string provider, string token)
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.CustomPolicyApi_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync();
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("apiservice");
        await resourceNotificationService
            .WaitForResourceAsync("apiservice", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(30));

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/ExternalUserData");
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Headers.Add("identity-provider", provider);

        var response = await httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain("email"));
        Assert.That(content, Does.Contain("@"));
    }
    
    [Test]
    public async Task PreLoginValidationWithValidMigratableUserReturnsOk()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.CustomPolicyApi_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync();
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("apiservice");
        await resourceNotificationService
            .WaitForResourceAsync("apiservice", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(30));

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/PreLoginValidation")
        {
            Content = new StringContent("""
                                        {
                                            "email": "your-test-user@example.com",
                                            "password": "correct-password"
                                        }
                                        """, Encoding.UTF8, "application/json")
        };

        // Act
        var response = await httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Expected OK but got {response.StatusCode}. Response content: {content}");
    }
}
