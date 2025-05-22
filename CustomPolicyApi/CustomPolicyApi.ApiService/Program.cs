using CustomPolicyApi.ApiService.DataAccess;
using CustomPolicyApi.ApiService.DataAccess.Contract;
using CustomPolicyApi.ApiService.Models;
using CustomPolicyApi.ApiService.UserExternalData;
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
builder.Services.AddScoped<IExternalUserDataService, ExternalUserDataService>();
builder.Services.AddScoped<IMsGraphDataAccess, MsGraphDataAccess>();
builder.Services.AddScoped<IAuth0DataAccess, Auth0DataAccess>();

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