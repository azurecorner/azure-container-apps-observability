using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection;

public static class ObserbilityServiceCollectionExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services,
        string serviceName,string sourceName,
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
                            //ptions.SetHttpFlavor = true;
                        })

                        .AddSqlClientInstrumentation(options =>
                        {
                            options.SetDbStatementForText = true; // 👈 Enables db.statement tag
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
                           .AddConsoleExporter()
                           .AddAzureMonitorTraceExporter(o =>
                           {
                               o.ConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
                           })

                          ;
            });

        //    services.AddLogging(
        //item => item.AddOpenTelemetry(options =>
        //{
        //    var connectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
        //    options
        //        .SetResourceBuilder(
        //            ResourceBuilder.CreateDefault()
        //                .AddService(serviceName)
        //                .AddAttributes(
        //                new List<KeyValuePair<string, object>>
        //                {
        //                    new  ("service.name", serviceName),
        //                    new  ("environment", "dev"),
        //                }))
        //        // send logs to Azure Monitor
        //        .AddAzureMonitorLogExporter(options =>
        //            options.ConnectionString = connectionString)
        //       // .AddConsoleExporter()
        //       ;
        //}));

        return services;
    }

    public static void MapObservability(this IEndpointRouteBuilder routes)
    {
        Console.WriteLine("==> MapPrometheusScrapingEndpoint");
        routes.MapPrometheusScrapingEndpoint();
    }
}