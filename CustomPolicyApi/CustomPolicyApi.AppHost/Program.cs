var builder = DistributedApplication.CreateBuilder(args);

// GitHub OAuth
var githubClientId = builder.AddParameter("OAuth:GitHub:ClientId");
var githubClientSecret = builder.AddParameter("OAuth:GitHub:ClientSecret", secret: true);

// Google OAuth
var googleClientId = builder.AddParameter("OAuth:Google:ClientId");
var googleClientSecret = builder.AddParameter("OAuth:Google:ClientSecret", secret: true);

// LinkedIn OAuth
var linkedinClientId = builder.AddParameter("OAuth:LinkedIn:ClientId");
var linkedinClientSecret = builder.AddParameter("OAuth:LinkedIn:ClientSecret", secret: true);

// Microsoft Graph
var msGraphClientId = builder.AddParameter("OAuth:MicrosoftGraph:ClientId");
var msGraphClientSecret = builder.AddParameter("OAuth:MicrosoftGraph:ClientSecret", secret: true);
var msGraphTenantId = builder.AddParameter("OAuth:MicrosoftGraph:TenantId");

// Auth0
var auth0ClientId = builder.AddParameter("OAuth:Auth0:ClientId");
var auth0ClientSecret = builder.AddParameter("OAuth:Auth0:ClientSecret", secret: true);
var auth0Domain = builder.AddParameter("OAuth:Auth0:Domain");

var apiService = builder.AddProject<Projects.CustomPolicyApi_ApiService>("apiservice")
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
    .WithEnvironment("OAuth__Auth0__Domain", auth0Domain);

builder.AddProject<Projects.CustomPolicyApi_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
