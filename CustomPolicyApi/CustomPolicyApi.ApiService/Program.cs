using CustomPolicyApi.ApiService.UserExternalData;
using CustomPolicyApi.ApiService.UserMigration;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // ✅ This enables console output
builder.Logging.SetMinimumLevel(LogLevel.Debug); // Or Information, depending on how much you want
builder.Logging.AddFilter("CustomPolicyApi", LogLevel.Debug);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IExternalUserDataService, ExternalUserDataService>();
builder.Services.AddScoped<IAuth0LoginService, Auth0LoginService>();
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();