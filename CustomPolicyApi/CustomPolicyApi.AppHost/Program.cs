var builder = DistributedApplication.CreateBuilder(args);

// GitHub
var githubClientId = builder.AddParameter("oauth-github-client-id");
var githubClientSecret = builder.AddParameter("oauth-github-client-secret", secret: true);

// Google
var googleClientId = builder.AddParameter("oauth-google-client-id");
var googleClientSecret = builder.AddParameter("oauth-google-client-secret", secret: true);

// LinkedIn
var linkedinClientId = builder.AddParameter("oauth-linkedin-client-id");
var linkedinClientSecret = builder.AddParameter("oauth-linkedin-client-secret", secret: true);

// Microsoft Graph
var msGraphClientId = builder.AddParameter("oauth-microsoftgraph-client-id");
var msGraphClientSecret = builder.AddParameter("oauth-microsoftgraph-client-secret", secret: true);
var msGraphTenantId = builder.AddParameter("oauth-microsoftgraph-tenant-id");

// Auth0
var auth0ClientId = builder.AddParameter("oauth-auth0-client-id");
var auth0ClientSecret = builder.AddParameter("oauth-auth0-client-secret", secret: true);
var auth0Domain = builder.AddParameter("oauth-auth0-domain");
var auth0Audience = builder.AddParameter("oauth-auth0-audience");

// Testing
var testEmail = builder.AddParameter("oauth-test-email", secret: true);
var testPassword = builder.AddParameter("oauth-test-password", secret: true);

var apiService = builder.AddProject<Projects.CustomPolicyApi_ApiService>("apiservice")
    //Testing email and password
    .WithEnvironment("OAuth__Test__Email", testEmail)
    .WithEnvironment("OAuth__Test__Password", testPassword)
    // GitHub
    .WithEnvironment("OAuth__GitHub__ClientId", githubClientId)
    .WithEnvironment("OAuth__GitHub__ClientSecret", githubClientSecret)

    // Google
    .WithEnvironment("OAuth__Google__ClientId", googleClientId)
    .WithEnvironment("OAuth__Google__ClientSecret", googleClientSecret)

    // LinkedIn
    .WithEnvironment("OAuth__LinkedIn__ClientId", linkedinClientId)
    .WithEnvironment("OAuth__LinkedIn__ClientSecret", linkedinClientSecret)

    // Microsoft Graph
    .WithEnvironment("OAuth__MicrosoftGraph__ClientId", msGraphClientId)
    .WithEnvironment("OAuth__MicrosoftGraph__ClientSecret", msGraphClientSecret)
    .WithEnvironment("OAuth__MicrosoftGraph__TenantId", msGraphTenantId)

    // Auth0
    .WithEnvironment("OAuth__Auth0__ClientId", auth0ClientId)
    .WithEnvironment("OAuth__Auth0__ClientSecret", auth0ClientSecret)
    .WithEnvironment("OAuth__Auth0__Domain", auth0Domain)
    .WithEnvironment("OAuth__Auth0__Audience", auth0Audience);

builder.Build().Run();
