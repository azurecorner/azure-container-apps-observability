using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using WeatherForecast.WebApi.Models;
using WeatherForecast.WebApi.Services;

namespace WebAppi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        //  https://api.open-meteo.com/v1/forecast?latitude=48.85&longitude=2.35&current_weather=true

        private HttpClient HttpClient;

        public IWeatherService WeatherService { get; }

        private readonly ILogger<WeatherForecastController> _logger;

        private static readonly ActivitySource ActivitySource = new("WeatherForecastWebApi");

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherService weatherService, HttpClient httpClient)
        {
            _logger = logger;

            WeatherService = weatherService;
            HttpClient = httpClient;
        }

        [HttpPost("{postalCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Add([FromRoute] int postalCode)
        {
            using var activity = ActivitySource.StartActivity("Create Delivery", ActivityKind.Server);

            Console.WriteLine("*************** WeatherForecastController.Add() ****************");
            Console.WriteLine("*************** StartActivity: Create Delivery ****************");

            if (activity != null)
            {
                activity.SetTag("http.route", "/WeatherForecast/{postalCode}");
                activity.SetTag("weather.city.postalCode", postalCode);
                Console.WriteLine($"TraceId: {activity.Context.TraceId}, SpanId: {activity.Context.SpanId}");
            }
            else
            {
                Console.WriteLine("Activity is null — trace context might not be propagated.");
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

                Console.WriteLine($"Température actuelle à {item.City} : {weatherData.CurrentWeather.Temperature}°C");

                await WeatherService.Add(weatherData);

                activity?.AddEvent(new ActivityEvent("Delivery Created", tags: new ActivityTagsCollection
        {
            new("weatherData.PostalCode", weatherData.PostalCode),
            new("weatherData.Elevation", weatherData.Elevation),
        }));
            }
            else
            {
                Console.WriteLine("Les données météo n'ont pas pu être récupérées.");
            }

            return Ok();
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get()
        {
            var result = await WeatherService.Get();

            return Ok(result);
        }
    }
}