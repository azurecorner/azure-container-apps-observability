using Microsoft.OpenApi.Models;
using OpenTelemetry.Trace;
using WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var serviceName = "WeatherForecast.WebApi";
string sourceName = "WebApi";
builder.Services.AddObservability(serviceName, sourceName, builder.Configuration);
// Register the metrics service.

builder.Services.AddHttpClient();
builder.Services.AddSingleton(TracerProvider.Default.GetTracer(serviceName));

builder.AddSerilog(serviceName, builder.Configuration);
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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