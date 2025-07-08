using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;
using Weather.Infrastructure.Metrics.Weather.Infrastructure.Metrics;
using WeatherForecast.WebApp.Models;

namespace WeatherForecast.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly TemperatureMetrics _temperatureMetrics;

        // Hardcoded for simulation
        private readonly Dictionary<string, string> _users = new()
        {
            { "admin", "password" },
            { "user", "1234" } // Add more users here as needed
        };
        public HomeController(ILogger<HomeController> logger, IHttpClientFactory clientFactory, TemperatureMetrics TemperatureMetrics)
        {
            _temperatureMetrics = TemperatureMetrics;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Diagnotics()
        {
            _logger.LogWarning("OTEL-APP => You are going to simulate diagnostic scenario, do not forget to remove this before going to prod !!!");
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }


        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (_users.TryGetValue(model.Username, out var expectedPassword) &&
                model.Password == expectedPassword)
            {
                HttpContext.Session.SetString("User", model.Username);
                _temperatureMetrics.TrackLoginSuccess(model.Username);
                return RedirectToAction("Index");
            }
            _temperatureMetrics.TrackLoginFailure(model.Username);
            model.ErrorMessage = "Invalid username or password.";
            return View(model);
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Remove("User");
            return RedirectToAction("Login");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}