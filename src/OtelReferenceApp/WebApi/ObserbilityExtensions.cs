using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace WebApi.Extensions
{
    public static class ObserbilityExtensions
    {
        public static IServiceCollection AddObservability(this IServiceCollection services,
            string serviceName, string sourceName,
            IConfiguration configuration)
        {
            // create the resource that references the service name passed in
            var resource = ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: "1.0")
                .AddAttributes(
                        new List<KeyValuePair<string, object>>
                        {
                        new  ("service.name", serviceName),
                        new  ("environment", "dev"),
                        });
            Console.WriteLine($"OLTP_ENDPOINT ==> {configuration["OLTP_ENDPOINT"]}");
            // add the OpenTelemetry services
            var otelBuilder = services.AddOpenTelemetry();

            otelBuilder
                // add the metrics providers
                .WithMetrics(metrics =>
                {
                    metrics
                  .SetResourceBuilder(resource)
                  .AddRuntimeInstrumentation()
                  .AddAspNetCoreInstrumentation()
                  .AddHttpClientInstrumentation()
                  .AddProcessInstrumentation()

                 .AddPrometheusExporter()

                 .AddOtlpExporter(options =>
                 {
                     var oltpEndpoint = "https://collector.ashycoast-13bf4b21.westeurope.azurecontainerapps.io/v1/metrics";
                     if (!string.IsNullOrEmpty(oltpEndpoint))
                     {
                         options.Endpoint = new Uri(oltpEndpoint);
                         options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                     }
                     else
                     {
                         throw new InvalidOperationException("OLTP_ENDPOINT configuration is missing or empty.");
                     }
                 });
                })
                // add the tracing providers
                .WithTracing(tracing =>
                {
                    tracing.SetResourceBuilder(resource).AddSource(sourceName)
                            .AddAspNetCoreInstrumentation()

                            .AddHttpClientInstrumentation(options =>
                            {
                                options.RecordException = true;
                                options.EnrichWithHttpRequestMessage = (activity, request) =>
                                {
                                    activity.SetTag("http.method", request.Method.Method);
                                    activity.SetTag("http.url", request?.RequestUri?.ToString());
                                };
                                options.FilterHttpRequestMessage = _ => true;
                                options.FilterHttpWebRequest = _ => true;
                            })

                            .AddSqlClientInstrumentation(options =>
                            {
                                options.SetDbStatementForText = true;
                                options.RecordException = true;
                            })
                            .AddEntityFrameworkCoreInstrumentation()

                               .AddOtlpExporter(options =>
                               {
                                   var oltpEndpoint = configuration["OLTP_ENDPOINT"];
                                   if (!string.IsNullOrEmpty(oltpEndpoint))
                                   {
                                       options.Endpoint = new Uri("https://collector.ashycoast-13bf4b21.westeurope.azurecontainerapps.io/v1/traces");
                                       options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                                   }
                                   else
                                   {
                                       throw new InvalidOperationException("OLTP_ENDPOINT configuration is missing or empty.");
                                   }
                               })

                              ;
                });

            return services;
        }

        public static void MapObservability(this IEndpointRouteBuilder routes)
        {
            Console.WriteLine("==> MapPrometheusScrapingEndpoint");
            routes.MapPrometheusScrapingEndpoint();
        }

        public static void AddSerilog(this WebApplicationBuilder builder, string serviceName)
        {
            builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
               loggerConfiguration
                   .ReadFrom.Configuration(hostingContext.Configuration)
                   .Enrich.FromLogContext()
                   .WriteTo.Console()

                    .WriteTo.OpenTelemetry(options =>
                    {
                        options.Endpoint = "https://collector.ashycoast-13bf4b21.westeurope.azurecontainerapps.io/v1/logs";
                        options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.HttpProtobuf;

                        options.ResourceAttributes = new Dictionary<string, object>
                        {
                            ["service.name"] = serviceName
                        };
                    }));
        }
    }
}