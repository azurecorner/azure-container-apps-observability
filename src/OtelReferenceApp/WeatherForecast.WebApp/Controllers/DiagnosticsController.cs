using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using WeatherForecast.WebApp.Models;

namespace WeatherForecast.WebApp.Controllers
{
    public class DiagnosticsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DiagnosticsController> _logger;
        public DiagnosticsController(IHttpClientFactory clientFactory, ILogger<DiagnosticsController> logger)
        {
            _httpClient = clientFactory.CreateClient("WebApi");
            _logger = logger;
        }

        public async Task<IActionResult> SimulateWebAppError()
        {
            try
            {
                var response = await _httpClient.GetAsync("WeatherForecast/simulate-error");
                response.EnsureSuccessStatusCode();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while simulating a Web App error.");
                return View("Error", new ErrorViewModel
                {
                    RequestId =  HttpContext.TraceIdentifier,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace ?? ex.InnerException?.StackTrace ?? string.Empty
                });
            }
        }

        public async Task<IActionResult> SimulateWebApiError()
        {
            try
            {
                var response = await _httpClient.GetAsync($"WeatherForecast/78888");

                response.EnsureSuccessStatusCode();
                
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while simulating a Web App error.");
                return View("Error", new ErrorViewModel
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace ?? ex.InnerException?.StackTrace ?? string.Empty
                });
            }
        }
    }

}
