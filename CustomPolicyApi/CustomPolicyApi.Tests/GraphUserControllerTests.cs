﻿
using System.Text;
using System.Text.Json;
using Aspire.Hosting;


namespace CustomPolicyApi.Tests;

public class MfaStatusResponse
{
    public bool MfaEnabled { get; set; }
}

public class GraphUserControllerTests
{
    private const string BaseEndpoint = "/api/GraphUser";
    private const string DefaultPassword = "Test1234!";

    private async Task<(DistributedApplication app, HttpClient httpClient)> StartAppAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.CustomPolicyApi_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        var app = await appHost.BuildAsync();
        await app.StartAsync();

        var resourceService = app.Services.GetRequiredService<ResourceNotificationService>();
        await resourceService
            .WaitForResourceAsync("apiservice", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(30));

        var httpClient = app.CreateHttpClient("apiservice");
        return (app, httpClient);
    }

    private static string GenerateUniqueEmail() => $"graphuser+{Guid.NewGuid():N}@sandboxcertiverse.onmicrosoft.com";

    [Test]
    public async Task CreateUser_ReturnsOk()
    {
        var (app, httpClient) = await StartAppAsync();
        await using var _ = app;

        var email = GenerateUniqueEmail();

        var response = await httpClient.PostAsync(BaseEndpoint, new StringContent(JsonSerializer.Serialize(new
        {
            email,
            password = DefaultPassword
        }), Encoding.UTF8, "application/json"));

        var content = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Response: {content}");
    }
    
    [Test]
    public async Task CreateUser_ReturnsOk_AndMfaIsDisabledByDefault()
    {
        var (app, httpClient) = await StartAppAsync();
        await using var _ = app;

        var email = GenerateUniqueEmail();

        // Act – Create user
        var createResponse = await httpClient.PostAsync(BaseEndpoint, new StringContent(JsonSerializer.Serialize(new
        {
            email,
            password = DefaultPassword
        }), Encoding.UTF8, "application/json"));

        var createContent = await createResponse.Content.ReadAsStringAsync();
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Create Response: {createContent}");

        // Act – Query MFA status
        var mfaResponse = await httpClient.GetAsync($"{BaseEndpoint}/{Uri.EscapeDataString(email)}/mfa-status");
        var mfaContent = await mfaResponse.Content.ReadAsStringAsync();

        Assert.That(mfaResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"MFA Status Response: {mfaContent}");

        var mfaResult = JsonSerializer.Deserialize<MfaStatusResponse>(mfaContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.That(mfaResult?.MfaEnabled, Is.False, "Expected MFA to be disabled by default.");
    }

    [Test]
    public async Task GetUserByEmail_ReturnsOk()
    {
        var (app, httpClient) = await StartAppAsync();
        await using var _ = app;

        var email = GenerateUniqueEmail();

        await httpClient.PostAsync(BaseEndpoint, new StringContent(JsonSerializer.Serialize(new
        {
            email,
            password = DefaultPassword
        }), Encoding.UTF8, "application/json"));

        var response = await httpClient.GetAsync($"{BaseEndpoint}/{email}");
        var content = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain(email));
    }

    [Test]
    public async Task DeleteUserByEmail_WorksCorrectly()
    {
        var (app, httpClient) = await StartAppAsync();
        await using var _ = app;

        var email = GenerateUniqueEmail();

        await httpClient.PostAsync(BaseEndpoint, new StringContent(JsonSerializer.Serialize(new
        {
            email,
            password = DefaultPassword
        }), Encoding.UTF8, "application/json"));

        var deleteResponse = await httpClient.DeleteAsync($"{BaseEndpoint}/{email}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var getResponse = await httpClient.GetAsync($"{BaseEndpoint}/{email}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task EnableUserMfa_ReturnsOk()
    {
        var (app, httpClient) = await StartAppAsync();
        await using var _ = app;

        var email = GenerateUniqueEmail();

        await httpClient.PostAsync(BaseEndpoint, new StringContent(JsonSerializer.Serialize(new
        {
            email,
            password = DefaultPassword
        }), Encoding.UTF8, "application/json"));

        var response = await httpClient.PostAsync($"{BaseEndpoint}/{email}/enable-mfa", null);
        var content = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Response: {content}");
        Assert.That(content, Does.Contain("MFA enabled"));
    }

    [Test]
    public async Task DisableUserMfa_ReturnsOk()
    {
        var (app, httpClient) = await StartAppAsync();
        await using var _ = app;

        var email = GenerateUniqueEmail();

        await httpClient.PostAsync(BaseEndpoint, new StringContent(JsonSerializer.Serialize(new
        {
            email,
            password = DefaultPassword
        }), Encoding.UTF8, "application/json"));

        var response = await httpClient.PostAsync($"{BaseEndpoint}/{email}/disable-mfa", null);
        var content = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Response: {content}");
        Assert.That(content, Does.Contain("MFA disabled"));
    }
}
