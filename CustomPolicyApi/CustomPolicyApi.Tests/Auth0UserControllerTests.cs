using System.Net;
using System.Text;
using System.Text.Json;
using Aspire.Hosting;

namespace CustomPolicyApi.Tests;

public class Auth0UserControllerTests
{
    private const string TestEmail = "test.integration.user@example.com";
    private const string TestPassword = "SecurePass123!";
    private const string CreateEndpoint = "/api/Auth0User";
    private const string DeleteEndpoint = $"/api/Auth0User/{TestEmail}";
    private HttpClient _httpClient = default!;
    private DistributedApplication _app = default!;
    private ResourceNotificationService _resourceNotificationService = default!;

    [SetUp]
    public async Task SetUp()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.CustomPolicyApi_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
        await using var app = await appHost.BuildAsync();
        _app = app;
        _resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        await _app.StartAsync();

        _httpClient = _app.CreateHttpClient("apiservice");

        await _resourceNotificationService
            .WaitForResourceAsync("apiservice", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(30));

        // Ensure the user exists before test
        var createRequest = new HttpRequestMessage(HttpMethod.Post, CreateEndpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                email = TestEmail,
                password = TestPassword
            }), Encoding.UTF8, "application/json")
        };

        var createResponse = await _httpClient.SendAsync(createRequest);
        if (createResponse.StatusCode != HttpStatusCode.OK && createResponse.StatusCode != HttpStatusCode.Conflict)
        {
            var msg = await createResponse.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create test user: {createResponse.StatusCode} - {msg}");
        }
    }

    [TearDown]
    public async Task TearDown()
    {
        _httpClient.Dispose();
        await _app.DisposeAsync();
    }

    [Test]
    public async Task GetUserByEmail_ReturnsOk()
    {
        var response = await _httpClient.GetAsync($"/api/Auth0User/{TestEmail}");
        var content = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain(TestEmail));
    }

    [Test]
    public async Task ValidateCredentials_ReturnsOk()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/Auth0User/validate")
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                email = TestEmail,
                password = TestPassword
            }), Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain("access_token").Or.Contain("id_token"));
    }

    [Test]
    public async Task DeleteUser_ReturnsNoContent()
    {
        var response = await _httpClient.DeleteAsync(DeleteEndpoint);
        var content = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent), $"Expected 204 NoContent but got {response.StatusCode}. Response: {content}");

        // Confirm user no longer exists
        var confirmResponse = await _httpClient.GetAsync($"/api/Auth0User/{TestEmail}");
        Assert.That(confirmResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task CreateUser_ReturnsOk()
    {
        // First delete to ensure clean slate
        await _httpClient.DeleteAsync(DeleteEndpoint);

        var createRequest = new HttpRequestMessage(HttpMethod.Post, CreateEndpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                email = TestEmail,
                password = TestPassword
            }), Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(createRequest);
        var content = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Expected OK but got {response.StatusCode}. Response: {content}");
    }
}
