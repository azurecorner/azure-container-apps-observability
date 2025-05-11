using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using WeatherForecast.Infrastructure.Models;

namespace WeatherForecast.WebApp.Controllers
{
    public class WeatherForecastController : Controller
    {
        public HttpClient HttpClient { get; }

        private static readonly ActivitySource ActivitySource = new("WeatherForecastWebApp");
        public WeatherForecastController(IHttpClientFactory clientFactory)
        {
            HttpClient = clientFactory. CreateClient("WebApi");
        }

        // GET: WeatherForecastController
        public async Task<ActionResult> Index()
        {
            using (var activity = ActivitySource.StartActivity("Get  List of WeatherForecast", ActivityKind.Client))
            {
                if (activity != null)
                {
                    activity.SetTag("http.route", "/WeatherForecast/index");
                    Console.WriteLine($"TraceId: {activity.Context.TraceId}, SpanId: {activity.Context.SpanId}");
                }
                else
                {
                    Console.WriteLine("Activity is null — trace context might not be propagated.");
                }
                // HTTP call to external weather API
                var response = await HttpClient.GetAsync("WeatherForecast");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                var weatherData = JsonSerializer.Deserialize<IEnumerable<LocationViewModel>>(content);
                return View(weatherData);
            }
            }

        // GET: WeatherForecastController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: WeatherForecastController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: WeatherForecastController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: WeatherForecastController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: WeatherForecastController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: WeatherForecastController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: WeatherForecastController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}