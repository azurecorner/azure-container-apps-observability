using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace WeatherForecast.Observability
{
    public static class SerilogConfiguration
    {
        public static void AddSerilog(this ILoggingBuilder loggingBuilder, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            loggingBuilder.AddSerilog();
        }

        public static void UseSerilog(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            string? serviceName = configuration["OpenTelemetry:Otlp:ServiceName"];
            if (serviceName is null)
            {
                throw new ArgumentNullException(nameof(serviceName));
            }

            builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                .ReadFrom.Configuration(hostingContext.Configuration)
                .WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = $"{configuration["OpenTelemetry:Otlp:Endpoint"]}/v1/logs";
                    options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc;

                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = serviceName
                    };
                }));
        }

        public static void AddSerilog(this WebApplicationBuilder builder, string serviceName)
        {
            builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
               loggerConfiguration
                   .ReadFrom.Configuration(hostingContext.Configuration)
                   .Enrich.FromLogContext()
                   .WriteTo.Console()
                   .WriteTo.ApplicationInsights(
                       hostingContext.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"],
                       TelemetryConverter.Traces) // Use TelemetryConverter.Traces for log traces
                   //.WriteTo.OpenTelemetry(options =>
                   //{
                   //    options.Endpoint = $"{hostingContext.Configuration["OLTP_ENDPOINT"]}/v1/logs";
                   //    options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc;

                   //    options.ResourceAttributes = new Dictionary<string, object>
                   //    {
                   //        ["service.name"] = serviceName
                   //    };
                   //})
                    .WriteTo.OpenTelemetry(options =>
                    {
                        options.Endpoint = "https://collector.bravetree-18daf065.westeurope.azurecontainerapps.io/v1/logs";
                        options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.HttpProtobuf;

                        options.ResourceAttributes = new Dictionary<string, object>
                        {
                            ["service.name"] = serviceName
                        };
                    }));
        }
    }
}