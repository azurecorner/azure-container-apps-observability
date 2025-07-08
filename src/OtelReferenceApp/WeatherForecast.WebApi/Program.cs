using Microsoft.OpenApi.Models;
using OpenTelemetry.Trace;
using Serilog;
using Weather.Infrastructure.Metrics.Weather.Infrastructure.Metrics;
using WeatherForecast.Infrastructure;
using WeatherForecast.Observability;
using WeatherForecast.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var meterName = "OtelReferenceApp.WeatherForecast";
var serviceName = "OtelReferenceApp.WeatherForecast.WebApi";
string sourceName = "WeatherForecastWebApi";
builder.Services.AddObservability(serviceName, sourceName, builder.Configuration, [meterName]);
// Register the metrics service.
builder.Services.AddSingleton<TemperatureMetrics>();

builder.Services.AddScoped<IWeatherService, WeatherService>();

builder.Services.RegisterInfrastureDependencies(builder.Configuration);
builder.Services.AddHttpClient();
builder.Services.AddSingleton(TracerProvider.Default.GetTracer(serviceName));
//logging

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.AddSerilog(serviceName);

builder.Services.AddControllers();

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

app.UseMiddleware<LoggingMiddleware>();

app.UsePathBase("/webapi");
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCors();
app.MapControllers();
app.MapObservability();

app.Run();