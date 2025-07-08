using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;
using System.Diagnostics;
using System.Text.Json;
using Weather.Infrastructure.Metrics.Weather.Infrastructure.Metrics;
using WeatherForecast.Observability;
using WeatherForecast.WebApi.Models;
using WeatherForecast.WebApi.Services;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.Http;

namespace WebAppi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        //  https://api.open-meteo.com/v1/forecast?latitude=48.85&longitude=2.35&current_weather=true

        private HttpClient HttpClient;
        private readonly Tracer _tracer;
 
        public IWeatherService WeatherService { get; }

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly TemperatureMetrics temperatureMetrics;

        private static readonly ActivitySource ActivitySource = new("WeatherForecastWebApi");

        public WeatherForecastController(ILogger<WeatherForecastController> logger, TemperatureMetrics TemperatureMetrics, IWeatherService weatherService, HttpClient httpClient, Tracer tracer)
        {
            _logger = logger;
            temperatureMetrics = TemperatureMetrics;
            WeatherService = weatherService;
            HttpClient = httpClient;
            _tracer = tracer;
        
         
        }

        [HttpPost("{postalCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Add([FromRoute] int postalCode)
        {
            try
            {
                using var activity = ActivitySource.StartActivity("Create Delivery", ActivityKind.Server);

                _logger.LogInformation("*************** WeatherForecastController.Add() ****************");
                _logger.LogInformation("*************** StartActivity: Create Delivery ****************");

                if (activity != null)
                {
                    activity.SetTag("http.route", "/WeatherForecast/{postalCode}");
                    activity.SetTag("weather.city.postalCode", postalCode);
                    _logger.LogInformation($"TraceId: {activity.Context.TraceId}, SpanId: {activity.Context.SpanId}");
                }
                else
                {
                    _logger.LogInformation("Activity is null — trace context might not be propagated.");
                }

                if (!ModelState.IsValid)
                    return BadRequest();

                var item = WeatherData.IleDeFranceWeatherData.SingleOrDefault(w => w.PostalCode == postalCode);

                if (item == null)
                    return NotFound(postalCode);

                // HTTP call to external weather API
                var response = await HttpClient.GetAsync(item.Url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var weatherData = JsonSerializer.Deserialize<WeatherForecastForCreationDto>(content, options);

                if (weatherData?.CurrentWeather != null)
                {
                    weatherData.Department = item.Department;
                    weatherData.DepartmentCode = item.DepartmentCode;
                    weatherData.PostalCode = item.PostalCode;
                    weatherData.City = item.City;

                    temperatureMetrics.IncrementWeatherReport(item.City);
                    temperatureMetrics.RecordTemperature(item.City, weatherData.CurrentWeather.Temperature);
                    temperatureMetrics.UpdateCurrentTemperature(item.City, weatherData.CurrentWeather.Temperature);

                    _logger.LogInformation($"Température actuelle à {item.City} : {weatherData.CurrentWeather.Temperature}°C");

                    await WeatherService.Add(weatherData);

                    activity?.AddEvent(new ActivityEvent("Delivery Created", tags: new ActivityTagsCollection
                    {
                        new("weatherData.PostalCode", weatherData.PostalCode),
                        new("weatherData.Elevation", weatherData.Elevation),
                    }));
                }
                else
                {
                    _logger.LogInformation("Les données météo n'ont pas pu être récupérées.");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest();
            }
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get()
        {
            // Remove all registered propagators to avoid conflicts.
            var propagators = new List<TextMapPropagator>();
            var compositePropagator = new CompositeTextMapPropagator(propagators);
            Sdk.SetDefaultTextMapPropagator(compositePropagator);

            var carrierContextPropagator = new SimpleTextMapPropagator();
            var propagationContext =
                carrierContextPropagator.ExtractActivityContext(default, HttpContext.Request.Headers,
                    (headers, name) => headers[name]);

            var spanContext = new SpanContext(propagationContext);
            using var WeatherSpan = _tracer.StartActiveSpan("WeatherProcessing", SpanKind.Server,
                spanContext);

            //WeatherSpan.SetAttribute("db-name", "prod-sql");
            //WeatherSpan.SetAttribute("connection-status", "success");
         
            WeatherSpan.SetStatus(Status.Ok);
            var result = await WeatherService.Get();
            WeatherSpan.SetAttribute("Query result count", result.Count);
            return Ok(result);
        }

        [HttpGet("{locationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get([FromRoute] int locationId)
        {
            var result = await WeatherService.Get(locationId);
            return Ok(result);
        }


    }
    public class SimpleTextMapPropagator
    {
        public ActivityContext ExtractActivityContext<T>(PropagationContext context, T carrier,
            Func<T, string, IEnumerable<string>> getter)
        {
            // 00-0af7651916cd43dd8448eb211c80319c-00f067aa0ba902b7-01

            var traceparent = getter(carrier, "traceparent")?.FirstOrDefault();
            if (traceparent == null) return default;

            var traceId = ActivityTraceId.CreateFromString(traceparent.Substring(3, 32).AsSpan());
            var spanId = ActivitySpanId.CreateFromString(traceparent.Substring(36, 16).AsSpan());

            var activityContext = new ActivityContext(traceId, spanId, ActivityTraceFlags.None);
            return activityContext;
        }
    }
}