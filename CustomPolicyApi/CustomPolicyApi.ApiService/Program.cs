using CustomPolicyApi.ApiService.Models;
using CustomPolicyApi.ApiService.TestingSetup;
using CustomPolicyApi.ApiService.UserExternalData;
using CustomPolicyApi.ApiService.UserMigration;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Logging.AddFilter("CustomPolicyApi", LogLevel.Debug);

// 🔐 Bind OAuth options (coming from AppHost WithEnvironment)
builder.Services.Configure<OAuthOptions>(builder.Configuration.GetSection("OAuth"));

// ➕ Aspire service defaults
builder.AddServiceDefaults();

// 🔧 System-wide services
builder.Services.AddProblemDetails();
builder.Services.AddHttpClient(); // Fallback HttpClient

// 🔐 Register typed OAuth-related services
builder.Services.AddHttpClient<IAuth0LoginService, Auth0LoginService>();
builder.Services.AddScoped<IGraphUserService, GraphUserService>();
builder.Services.AddScoped<IExternalUserDataService, ExternalUserDataService>();
builder.Services.AddScoped<IAuth0UserLookupService, Auth0UserLookupService>();

builder.Services.AddControllers();

// 🌐 OpenAPI + Scalar API Reference
builder.Services.AddOpenApi();

var app = builder.Build();

// ⚠️ Centralized error handling
app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference();
app.MapControllers();
app.MapDefaultEndpoints();

app.Run();