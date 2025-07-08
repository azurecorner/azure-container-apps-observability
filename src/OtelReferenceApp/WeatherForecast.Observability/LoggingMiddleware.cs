using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherForecast.Observability
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Request Logging
            _logger.LogInformation("Handling request: {method} {url}", context.Request.Method, context.Request.Path);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OTEL-APP => An unhandled exception has occurred: {message}", ex.Message);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal Server Error");
            }

            // Response Logging
            _logger.LogInformation("OTEL-APP => Finished handling request. Response Status Code: {statusCode}", context.Response.StatusCode);
        }
    }

}
