using System.Text;
using System.Text.Json;

namespace CustomPolicyApi.Tests;

public class WebTests
{

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
