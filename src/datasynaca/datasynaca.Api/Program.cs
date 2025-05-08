using Azure.Monitor.OpenTelemetry.AspNetCore;
using Healthtrackr.Api.Extensions;
using Healthtrackr.Api.Services;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Logging.AddOpenTelemetry(x =>
{
    x.IncludeScopes = true;
    x.IncludeFormattedMessage = true;
});
builder.Services.AddOpenTelemetry()
    .UseAzureMonitor()
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation();
    });
builder.Services.AddSingleton<IWeightManager, FakeWeightManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.RegisterWeightEndpoints();

app.Run();
