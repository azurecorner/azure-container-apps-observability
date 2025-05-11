using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace WeatherForecast.Observability
{
    public static class SerilogConfiguration
    {
        public static void AddSerilog(this WebApplicationBuilder builder, string serviceName)
        {
            builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
               loggerConfiguration
                   .ReadFrom.Configuration(hostingContext.Configuration)
                   .Enrich.FromLogContext()
                   .WriteTo.Console()
                   //.WriteTo.ApplicationInsights(
                   //    hostingContext.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"],
                   //    TelemetryConverter.Traces) // Use TelemetryConverter.Traces for log traces
                   //.WriteTo.OpenTelemetry(options =>
                   //{
                   //    options.Endpoint = $"{hostingContext.Configuration["OLTP_ENDPOINT"]}/v1/logs";
                   //    options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc;

                   //    options.ResourceAttributes = new Dictionary<string, object>
                   //    {
                   //        ["service.name"] = serviceName
                   //    };
                   //})
                   );
        }
    }
}