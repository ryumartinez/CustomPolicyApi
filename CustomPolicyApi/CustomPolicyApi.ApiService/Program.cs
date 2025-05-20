using CustomPolicyApi.ApiService.UserExternalData;
using CustomPolicyApi.ApiService.UserMigration;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // ✅ Enables console output
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Logging.AddFilter("CustomPolicyApi", LogLevel.Debug);

// 🔐 Bind Auth0 options from configuration (appsettings.json or environment variables)
builder.Services.Configure<Auth0Options>(builder.Configuration.GetSection("Auth0"));

// Add service defaults & Aspire client integrations
builder.AddServiceDefaults();

// Add services to the container
builder.Services.AddProblemDetails();
builder.Services.AddHttpClient(); // Generic fallback client

// ✅ Register the typed HttpClient for Auth0 login
builder.Services.AddHttpClient<IAuth0LoginService, Auth0LoginService>();

builder.Services.AddScoped<IExternalUserDataService, ExternalUserDataService>();
builder.Services.AddControllers();

// 🌐 OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// 🔥 Exception handler
app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();