using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using WeatherForecast.Infrastructure.Models;
using OpenTelemetry.Trace;
using NuGet.DependencyResolver;
using System.Net.Http;

namespace WeatherForecast.WebApp.Controllers
{
    public class WeatherForecastController : Controller
    {
        public ILogger<WeatherForecastController> Logger { get; }

        //private static readonly ActivitySource ActivitySource = new("WeatherForecastWebApp");

        private readonly HttpClient _httpClient;
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly Tracer _tracer;

        public WeatherForecastController(IHttpClientFactory clientFactory, ILogger<WeatherForecastController> logger, Tracer tracer)
        {
            _httpClient = clientFactory.CreateClient("WebApi");
            _logger = logger;
            _tracer = tracer;
        }

        // GET: WeatherForecastController
        public async Task<ActionResult> Index()
        {


            //if (activity != null)
            //{
            //    activity.SetTag("http.route", "/WeatherForecast/index");
            //    activity.SetTag("http.method", "GET");
            //    activity.SetTag("http.url", "WeatherForecast");
            //    _logger.LogInformation("Started activity for WeatherForecast with TraceId: {TraceId}, SpanId: {SpanId}",
            //        activity.Context.TraceId, activity.Context.SpanId);
            //}
            //else
            //{
            //    _logger.LogWarning("Activity is null — trace context might not be propagated.");
            //}

            try
            {
                using var httpSpan = _tracer.StartActiveSpan("Making HTTP Call", SpanKind.Client);
                httpSpan.SetAttribute("http.route", "/WeatherForecast/index");
                httpSpan.SetAttribute("protocol", "http");
        
              

                var weatherRequest = new HttpRequestMessage(HttpMethod.Get, "WeatherForecast");
               // var response = await _httpClient.GetAsync("WeatherForecast");

                // Pass Trace Context to weather Service
                var propagator = new TraceContextPropagator();

                var parentSpanContext = httpSpan.Context;
                var activity = Activity.Current?.SetParentId(parentSpanContext.TraceId, parentSpanContext.SpanId);

                if (activity != null)
                {
                    _logger.LogInformation("Started activity for WeatherForecast with TraceId: {TraceId}, SpanId: {SpanId}", activity.Context.TraceId, activity.Context.SpanId);
                    var propagationContext = new PropagationContext(activity.Context, Baggage.Current);

                    propagator.Inject(propagationContext, weatherRequest.Headers,
                        (headers, name, value) => { headers.Add(name, value); });
                }

                // End passing trace context
            
                    Console.WriteLine($"Calling weather Service at WeatherForecast");
                    var response  = await _httpClient.SendAsync(weatherRequest);
                    var content = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var weatherData = JsonSerializer.Deserialize<IEnumerable<LocationInListViewModel>>(content, options);

                    _logger.LogInformation("Successfully fetched weather data. Items count: {Count}", weatherData?.Count() ?? 0);
                httpSpan.SetStatus(Status.Ok);
                activity?.Stop();
                return View(weatherData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occurred while fetching weather data.");
                    // activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    return StatusCode(500, "Internal server error");
                }


              

              
        }


        // GET: WeatherForecastController/Details/Id
        public async Task<ActionResult> Details(int Id)
        {
            var response = await _httpClient.GetAsync($"WeatherForecast/{Id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound(); // ou afficher une vue d’erreur personnalisée
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var weatherData = JsonSerializer.Deserialize<LocationInListViewModel>(content, options);

            if (weatherData == null)
            {
                return BadRequest("Invalid data received from the API.");
            }

            return View(weatherData);
        }

        // GET: WeatherForecastController/Create
        public ActionResult Create()
        {
            var model = new WeatherForecastCreateViewModel
            {
                PostalCodeOptions = GetPostalCodeSelectList()
            };
            return View(model);
        }

        // POST: WeatherForecastController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(WeatherForecastCreateViewModel model)
        {
           

            //if (activity != null)
            //{
            //    activity.SetTag("http.route", "/WeatherForecast/index");
            //    activity.SetTag("http.method", "POST");
            //    activity.SetTag("http.url", "WeatherForecast");
            //    activity.SetTag("PostalCode", model.PostalCode);
            //    _logger.LogInformation("Started activity for WeatherForecast creation with TraceId: {TraceId}, SpanId: {SpanId}",
            //        activity.Context.TraceId, activity.Context.SpanId);
            //}
            //else
            //{
            //    _logger.LogWarning("Activity is null — trace context might not be propagated.");
            //}
            model.PostalCodeOptions = GetPostalCodeSelectList();

            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                // create the request
                var request = new HttpRequestMessage(HttpMethod.Post, $"WeatherForecast/{model.PostalCode}")
                {
                    Content = new StringContent(string.Empty, Encoding.UTF8, "application/json")
                };

               
                // send the request
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(model);
            }
        }

        private IEnumerable<SelectListItem> GetPostalCodeSelectList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "Select...", Value = "", Disabled = true, Selected = true },
                new SelectListItem { Text = "75000 - Paris", Value = "75000" },
                new SelectListItem { Text = "91000 - Essonne (Évry-Courcouronnes)", Value = "91000" },
                new SelectListItem { Text = "92000 - Hauts-de-Seine (Nanterre)", Value = "92000" },
                new SelectListItem { Text = "93000 - Seine-Saint-Denis (Bobigny)", Value = "93000" },
                new SelectListItem { Text = "94000 - Val-de-Marne (Créteil)", Value = "94000" },
                new SelectListItem { Text = "95000 - Val-d'Oise (Cergy)", Value = "95000" },
                new SelectListItem { Text = "77000 - Seine-et-Marne (Melun)", Value = "77000" },
                new SelectListItem { Text = "78000 - Yvelines (Versailles)", Value = "78000" }
            };
        }
    }
}