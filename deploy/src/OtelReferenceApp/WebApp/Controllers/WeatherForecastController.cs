using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class WeatherForecastController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(IHttpClientFactory clientFactory, ILogger<WeatherForecastController> logger)
        {
            _httpClient = clientFactory.CreateClient("WebApi");
            _logger = logger;
        }

        // GET: WeatherForecastController
        public async Task<ActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("WeatherForecast");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to retrieve weather data. Status code: {StatusCode}", response.StatusCode);
                    return StatusCode((int)response.StatusCode);
                }

                var content = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var weatherData = JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(content, options);

                _logger.LogInformation("Successfully fetched weather data. Items count: {Count}", weatherData?.Count() ?? 0);

                return View(weatherData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching weather data.");

                return StatusCode(500, "Internal server error");
            }
        }
    }
}