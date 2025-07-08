using LogisticManagement.Infrastructure.Repositories;
using OpenTelemetry.Trace;
using System.Diagnostics;
using WeatherForecast.WebApi.Models;
using WeatherForecast.WebApi.Models.Creation;
using WeatherForecast.WebApi.WeatherForecast.Application.Mappers;
using WebAppi.Controllers;

namespace WeatherForecast.WebApi.Services;

public class WeatherService : IWeatherService
{
    private readonly Tracer _tracer;

    private IWeatherRepository WeatherRepository { get; }

    //private static readonly ActivitySource Activity = new("WeatherForecastWebApi");

    public WeatherService(IWeatherRepository weatherRepository, Tracer tracer)
    {
        WeatherRepository = weatherRepository;
        _tracer = tracer;
    }

    public async Task Add(WeatherForecastForCreationDto weatherDto)
    {
        //// Démarrage de l'activité principale
        //using var activity = Activity.StartActivity("Insert Weather Data", ActivityKind.Internal);

        //if (activity != null)
        //{
        //    Console.WriteLine($"Current activity found. TraceId: {activity.Context.TraceId}, SpanId: {activity.Context.SpanId}");
        //}
        //else
        //{
        //    Console.WriteLine("Failed to start new activity. The trace context might not be properly propagated.");
        //}

        // Conversion des données du DTO
        var location = weatherDto.ToLocation();

        // Propagation du contexte pour l'activité d'insertion dans la base de données
        //using (var dbActivity = Activity.StartActivity("Insert into DB", ActivityKind.Internal, parentContext: activity.Context))
        //{
        //    Console.WriteLine("***************************  WEATHER SERVICE  ( StartActivity Insert Database ) ***********************************************");

        //    if (dbActivity != null)
        //    {
        //        Console.WriteLine($"Current activity found. TraceId: {dbActivity.Context.TraceId}, SpanId: {dbActivity.Context.SpanId}");
        //    }
        //    else
        //    {
        //        Console.WriteLine("Failed to start new activity. The trace context might not be properly propagated.");
        //    }

            // Appel à la base de données
            await WeatherRepository.Add(location, location.Weather);
        //}
    }

    public async Task<List<WeatherForecastInListDto>> Get()
    {
        using var httpSpan = _tracer.StartActiveSpan("Making HTTP Call", SpanKind.Client);
        httpSpan.SetAttribute("db", "WeatherForecastDb");
    
        var activity = Activity.Current;

        if (activity != null)
        {
            activity.SetTag("db", "WeatherForecastDb");
            
            Console.WriteLine($"Started activity for WeatherForecast with TraceId: {activity.Context.TraceId}, SpanId: {activity.Context.SpanId}");
        }
        else
        {
            Console.WriteLine("Activity is null — trace context might not be propagated.");
        }

            var result = await WeatherRepository.GetAll();
        activity?.SetTag("rows", result.Count());
        return result.Select(x => x.ToListDto()).ToList();
    }



    public async Task<WeatherForecastInListDto> Get(int locationId)
    {
        var result = await WeatherRepository.Get(locationId);

        if (result == null)
        {
            throw new ArgumentNullException(nameof(result), "Location not found for the given locationId.");
        }

        return result.ToListDto();
    }
}