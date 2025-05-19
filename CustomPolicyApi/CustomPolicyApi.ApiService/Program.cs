using CustomPolicyApi.ApiService.UserExternalData;
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
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference();

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
