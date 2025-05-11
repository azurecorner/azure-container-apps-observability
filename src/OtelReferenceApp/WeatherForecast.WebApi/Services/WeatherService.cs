using LogisticManagement.Infrastructure.Repositories;
using System.Diagnostics;
using WeatherForecast.WebApi.Models;
using WeatherForecast.WebApi.Models.Creation;
using WeatherForecast.WebApi.WeatherForecast.Application.Mappers;

namespace WeatherForecast.WebApi.Services;

public class WeatherService : IWeatherService
{
    private IWeatherRepository WeatherRepository { get; }

    private static readonly ActivitySource Activity = new("WeatherForecastWebApi");

    public WeatherService(IWeatherRepository weatherRepository)
    {
        WeatherRepository = weatherRepository;
    }

    public async Task Add(WeatherForecastForCreationDto weatherDto)
    {
        // Démarrage de l'activité principale
        using var activity = Activity.StartActivity("Insert Weather Data", ActivityKind.Internal);

        if (activity != null)
        {
            Console.WriteLine($"Current activity found. TraceId: {activity.Context.TraceId}, SpanId: {activity.Context.SpanId}");
        }
        else
        {
            Console.WriteLine("Failed to start new activity. The trace context might not be properly propagated.");
        }

        // Conversion des données du DTO
        var location = weatherDto.ToLocation();

        // Propagation du contexte pour l'activité d'insertion dans la base de données
        using (var dbActivity = Activity.StartActivity("Insert into DB", ActivityKind.Internal, parentContext: activity.Context))
        {
            Console.WriteLine("***************************  WEATHER SERVICE  ( StartActivity Insert Database ) ***********************************************");

            if (dbActivity != null)
            {
                Console.WriteLine($"Current activity found. TraceId: {dbActivity.Context.TraceId}, SpanId: {dbActivity.Context.SpanId}");
            }
            else
            {
                Console.WriteLine("Failed to start new activity. The trace context might not be properly propagated.");
            }

            // Appel à la base de données
            await WeatherRepository.Add(location, location.Weather);
        }
    }

    public async Task<List<WeatherForecastInListDto>> Get()
    {
        var result = await WeatherRepository.GetAll();
        return result.Select(x => x.ToListDto()).ToList();
    }
}