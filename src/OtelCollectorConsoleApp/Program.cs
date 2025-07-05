// Program.cs
using System;
using System.Threading.Tasks;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;


        // 2. Build a tracer provider that sends spans directly to your collector endpoint
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("ConsoleOtlpService"))
            .AddSource("ConsoleOtlpService")
            .AddOtlpExporter(options =>
            {
                // hard-coded OTLP HTTP endpoint with /v1/traces path
                options.Endpoint = new Uri("https://collector.blueground-9681b7a3.uksouth.azurecontainerapps.io/v1/traces");
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            })
            .Build();

        var tracer = tracerProvider.GetTracer("ConsoleOtlpService");

        // 3. Create a sample span
        using (var span = tracer.StartActiveSpan("SampleSpan"))
        {
            span.SetAttribute("example.key", "example.value");
            Console.WriteLine("Span started. Press any key to end it.");
         
        }

        // 4. Wait for the exporter to flush
        await Task.Delay(1000);
