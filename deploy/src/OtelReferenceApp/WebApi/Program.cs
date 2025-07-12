using Microsoft.OpenApi.Models;
using Observability;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var serviceName = "WeatherForecast.WebApi";
string sourceName = "WebApi";
builder.Services.AddObservability(serviceName, sourceName, builder.Configuration);
builder.AddSerilog(serviceName, builder.Configuration);

builder.Services.AddHttpClient();
builder.Services.AddSingleton(TracerProvider.Default.GetTracer(serviceName));

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