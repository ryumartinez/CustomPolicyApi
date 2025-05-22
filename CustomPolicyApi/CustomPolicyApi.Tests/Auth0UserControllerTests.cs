using System.Net;
using System.Text;
using System.Text.Json;
using Aspire.Hosting;

namespace CustomPolicyApi.Tests;

public class Auth0UserControllerTests
{
    private const string CreateEndpoint = "/api/Auth0User";
    private const string ValidateEndpoint = "/api/Auth0User/validate";

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

    private static string GenerateUniqueEmail() =>
        $"testuser+{Guid.NewGuid():N}@example.com";

    private const string DefaultPassword = "Test1234!";

    [Test]
    public async Task GetUserByEmail_ReturnsOk()
    {
        var (app, httpClient) = await StartAppAsync();
        await using var _ = app;

        var email = GenerateUniqueEmail();
        var password = DefaultPassword;

        await httpClient.PostAsync(CreateEndpoint, new StringContent(JsonSerializer.Serialize(new
        {
            email,
            password
        }), Encoding.UTF8, "application/json"));

        var response = await httpClient.GetAsync($"/api/Auth0User/{email}");
        var content = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain(email));
    }

    [Test]
    public async Task ValidateCredentials_ReturnsOk()
    {
        var (app, httpClient) = await StartAppAsync();
        await using var _ = app;

        var email = GenerateUniqueEmail();
        var password = DefaultPassword;

        // Create user
        await httpClient.PostAsync(CreateEndpoint, new StringContent(JsonSerializer.Serialize(new
        {
            email,
            password
        }), Encoding.UTF8, "application/json"));

        // Validate
        var request = new HttpRequestMessage(HttpMethod.Post, ValidateEndpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                email,
                password
            }), Encoding.UTF8, "application/json")
        };

        var response = await httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain("access_token").Or.Contain("id_token"));
    }

    [Test]
    public async Task DeleteUser_ReturnsNoContent()
    {
        var (app, httpClient) = await StartAppAsync();
        await using var _ = app;

        var email = GenerateUniqueEmail();
        var password = DefaultPassword;

        // Create user
        await httpClient.PostAsync(CreateEndpoint, new StringContent(JsonSerializer.Serialize(new
        {
            email,
            password
        }), Encoding.UTF8, "application/json"));

        // Delete user
        var deleteResponse = await httpClient.DeleteAsync($"/api/Auth0User/{email}");
        var content = await deleteResponse.Content.ReadAsStringAsync();

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent), $"Expected 204 NoContent but got {deleteResponse.StatusCode}. Response: {content}");

        // Confirm deletion
        var confirmResponse = await httpClient.GetAsync($"/api/Auth0User/{email}");
        Assert.That(confirmResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task CreateUser_ReturnsOk()
    {
        var (app, httpClient) = await StartAppAsync();
        await using var _ = app;

        var email = GenerateUniqueEmail();
        var password = DefaultPassword;

        var createRequest = new HttpRequestMessage(HttpMethod.Post, CreateEndpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                email,
                password
            }), Encoding.UTF8, "application/json")
        };

        var response = await httpClient.SendAsync(createRequest);
        var content = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Expected OK but got {response.StatusCode}. Response: {content}");
    }
}
