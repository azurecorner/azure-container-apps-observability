using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection;

public static class ObserbilityServiceCollectionExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services,
        string serviceName, string sourceName,
        IConfiguration configuration, string[]? meeterNames = null)
    {
        // create the resource that references the service name passed in
        var resource = ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: "1.0")
            .AddAttributes(
                    new List<KeyValuePair<string, object>>
                    {
                        new  ("service.name", serviceName),
                        new  ("environment", "dev"),
                    });
        var meters = meeterNames != null ? string.Join("-", meeterNames) : "none";
        Console.WriteLine($"Adding observability for {serviceName} with version 1.0 and meters {meters}");
        Console.WriteLine($"APPLICATIONINSIGHTS_CONNECTION_STRING ==> {configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]}");
        Console.WriteLine($"JAEGER_URL ==> {configuration["JAEGER_URL"]}");
        Console.WriteLine($"ZIPKIN_URL ==> {configuration["ZIPKIN_URL"]}");
        Console.WriteLine($"OLTP_ENDPOINT ==> {configuration["OLTP_ENDPOINT"]}");
        // add the OpenTelemetry services
        var otelBuilder = services.AddOpenTelemetry();

        if (!string.IsNullOrEmpty(configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        {
            otelBuilder.UseAzureMonitor(options =>
            {
                options.ConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
            }
                );
        }

        var tracingOtlpEndpoint = configuration["JAEGER_URL"];

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

              .AddMeter(meeterNames ?? [])
              .AddView("temperature-distribution", new ExplicitBucketHistogramConfiguration
              {
                  Boundaries = new double[] { 0, 5, 10, 15, 20, 25, 30, 40, 50 } // realistic for Celsius
              })
             //.AddConsoleExporter()
             .AddPrometheusExporter()
             .AddAzureMonitorMetricExporter(o =>
             {
                 o.ConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
             })
             .AddOtlpExporter(options =>
             {
                 var oltpEndpoint = configuration["OLTP_ENDPOINT"];
                 if (!string.IsNullOrEmpty(oltpEndpoint))
                 {
                     options.Endpoint = new Uri(oltpEndpoint); // aks use http://open-telemetry-collector.monitoring.svc.cluster.local:4318
                 }
                 else
                 {
                     throw new InvalidOperationException("OLTP_ENDPOINT configuration is missing or empty.");
                 }
             })
             .AddOtlpExporter(options =>
             {
                 var oltpEndpoint = "https://collector.delightfulmoss-8b2af8d8.westeurope.azurecontainerapps.io/v1/metrics";
                 if (!string.IsNullOrEmpty(oltpEndpoint))
                 {
                     options.Endpoint = new Uri(oltpEndpoint); // aks use http://open-telemetry-collector.monitoring.svc.cluster.local:4318
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
                                activity.SetTag("http.url", request.RequestUri.ToString());
                            };
                            //options.RecordException = true;
                           
                            // Ensure parent-child relationships are maintained
                            options.FilterHttpRequestMessage = _ => true;
                            options.FilterHttpWebRequest = _ => true;
                        })

                        .AddSqlClientInstrumentation(options =>
                        {
                            options.SetDbStatementForText = true;
                            options.RecordException = true;
                        })
                        .AddEntityFrameworkCoreInstrumentation()
                          .AddZipkinExporter(zipkin =>
                          {
                              var zipkinUrl = configuration["ZIPKIN_URL"] ?? "http://zipkin:9411";
                              zipkin.Endpoint = new Uri($"{zipkinUrl}/api/v2/spans");
                          })

                          .AddOtlpExporter(options =>
                          {
                              var oltpEndpoint = configuration["OLTP_ENDPOINT"];
                              if (!string.IsNullOrEmpty(oltpEndpoint))
                              {
                                  options.Endpoint = new Uri(oltpEndpoint); ; // aks use http://open-telemetry-collector.monitoring.svc.cluster.local:4318
                              }
                              else
                              {
                                  throw new InvalidOperationException("OLTP_ENDPOINT configuration is missing or empty.");
                              }
                          })
                           .AddOtlpExporter(options =>
                           {
                               var oltpEndpoint = configuration["OLTP_ENDPOINT"];
                               if (!string.IsNullOrEmpty(oltpEndpoint))
                               {
                                   options.Endpoint = new Uri("https://collector.delightfulmoss-8b2af8d8.westeurope.azurecontainerapps.io/v1/traces");
                                   options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                               }
                               else
                               {
                                   throw new InvalidOperationException("OLTP_ENDPOINT configuration is missing or empty.");
                               }
                           })
                          .AddOtlpExporter(options =>
                          {
                              var tempoEndpoint = configuration["TEMPO_OTLP_ENDPOINT"];
                              if (string.IsNullOrEmpty(tempoEndpoint))
                              {
                                  throw new InvalidOperationException("TEMPO_OTLP_ENDPOINT is missing.");
                              }

                              // Ensure the endpoint includes the /v1/traces path.
                              if (!tempoEndpoint.EndsWith("/v1/traces", StringComparison.OrdinalIgnoreCase))
                              {
                                  tempoEndpoint = $"{tempoEndpoint.TrimEnd('/')}/v1/traces";
                              }

                              options.Endpoint = new Uri(tempoEndpoint);
                              options.Protocol = OtlpExportProtocol.HttpProtobuf;
                          })
                           //.AddConsoleExporter()
                           .AddAzureMonitorTraceExporter(o =>
                           {
                               o.ConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
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

  
}