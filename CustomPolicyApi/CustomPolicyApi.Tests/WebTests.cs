namespace CustomPolicyApi.Tests;

public class WebTests
{
    [Test]
    public async Task GetWebResourceRootReturnsOkStatusCode()
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
        var httpClient = app.CreateHttpClient("webfrontend");
        await resourceNotificationService.WaitForResourceAsync("webfrontend", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
        var response = await httpClient.GetAsync("/");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [TestCase("github", "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")]
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
        var httpClient = app.CreateHttpClient("apiservice"); // ✅ FIX: was "webfrontend"
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
}
