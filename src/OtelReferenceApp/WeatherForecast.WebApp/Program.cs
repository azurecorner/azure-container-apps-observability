using OpenTelemetry.Trace;
using Weather.Infrastructure.Metrics.Weather.Infrastructure.Metrics;
using WeatherForecast.Observability;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddHttpClient("WebApi", client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["WEBAPI_URL"] ?? throw new InvalidOperationException("WEBAPI_URL configuration is missing or empty."));
        });

        // Register the metrics service.
        builder.Services.AddSingleton<TemperatureMetrics>();
 
        var serviceName = "OtelReferenceApp.WeatherForecast.WebApp";
        string[] meterName = ["OtelReferenceApp.WeatherForecast"];
        string sourceName = "WeatherForecastWebApp";
        builder.Services.AddObservability(serviceName, sourceName, builder.Configuration, meterName);
        builder.Services.AddSingleton(TracerProvider.Default.GetTracer(serviceName));

        builder.AddSerilog(serviceName);

        builder.Services.AddSession();
        var app = builder.Build();

        app.UseMiddleware<LoggingMiddleware>();

        // Enable session middleware
        app.UseSession();
        // Configure the HTTP request pipeline.

        //app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.

        app.UsePathBase("/webapp");


        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthorization();

        app.MapStaticAssets();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();
        app.MapObservability();
        app.Run();
    }
}