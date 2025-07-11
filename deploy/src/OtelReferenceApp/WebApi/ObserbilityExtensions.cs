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

            var oltpEndpoint = $"{configuration["OLTP_ENDPOINT"]}" ?? throw new InvalidOperationException("OLTP_ENDPOINT FOR METRICS AND TRACES configuration is missing or empty.");

            Console.WriteLine($"OLTP_ENDPOINT FOR METRICS AND TRACES ==> {oltpEndpoint}");
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
                     options.Endpoint = new Uri($"{oltpEndpoint}/v1/metrics");
                     options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
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
                                   options.Endpoint = new Uri($"{oltpEndpoint}/v1/traces");
                                   options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
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

        public static void AddSerilog(this WebApplicationBuilder builder, string serviceName, IConfiguration configuration)
        {
            var oltpEndpoint = $"{configuration["OLTP_ENDPOINT"]}" ?? throw new InvalidOperationException("OLTP_ENDPOINT FOR LOGS configuration is missing or empty.");
            Console.WriteLine($"OLTP_ENDPOINT FOR LOGS ==> {oltpEndpoint}");
            builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
               loggerConfiguration
                   .ReadFrom.Configuration(hostingContext.Configuration)
                   .Enrich.FromLogContext()
                   .WriteTo.Console()

                    .WriteTo.OpenTelemetry(options =>
                    {
                        options.Endpoint = $"{oltpEndpoint}/v1/logs";
                        options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.HttpProtobuf;

                        options.ResourceAttributes = new Dictionary<string, object>
                        {
                            ["service.name"] = serviceName
                        };
                    }));
        }
    }
}