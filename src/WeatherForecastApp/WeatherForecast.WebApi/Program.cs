using Microsoft.OpenApi.Models;
using Serilog;
using WeatherForecast.Infrastructure;
using WeatherForecast.Observability;
using WeatherForecast.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var meterName = "OtelReferenceApp.WeatherForecast";//builder.Configuration.GetValue<string>("LogisticManagementMeterName") ?? "Logistic.Delivery";
var serviceName = "OtelReferenceApp.WeatherForecast.WebApi";
string sourceName = "WeatherForecastWebApi";
builder.Services.AddObservability(serviceName, sourceName, builder.Configuration, [meterName]);
// Register the metrics service.
builder.Services.AddSingleton<WeatherMetrics>();

builder.Services.AddScoped<IWeatherService, WeatherService>();

builder.Services.RegisterInfrastureDependencies(builder.Configuration);
builder.Services.AddHttpClient();

//logging

//builder.Logging.AddOpenTelemetry(options =>
//{
//    options.IncludeFormattedMessage = true;
//    options.IncludeScopes = true;
//});

builder.AddSerilog(serviceName);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "LogisticManagement API",
        Version = "v1"
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapObservability();

app.Run();