using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WeatherForecast.Infrastructure.Models;

namespace LogisticManagement.Infrastructure.Repositories
{
    public class WeatherRepository : IWeatherRepository
    {
        private WeatherForecastDbContext Context;

        private static readonly ActivitySource Activity = new("WeatherController");

        public WeatherRepository(WeatherForecastDbContext context)
        {
            Context = context;
        }

        public async Task Add(Location item)
        {
            // Démarrer une activité pour l'insertion dans la base de données
            using var activity = new ActivitySource("WeatherController").StartActivity("Insert Location into DB", ActivityKind.Internal);

            if (activity != null)
            {
                Console.WriteLine($"Current activity found. TraceId: {activity.Context.TraceId}, SpanId: {activity.Context.SpanId}");
            }
            else
            {
                Console.WriteLine("Failed to start new activity. The trace context might not be properly propagated.");
            }

            // Effectuer l'opération d'ajout dans la base de données

            await Context.Location.AddAsync(item);
            await Context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Location>> GetAll()
        {
            return await Context.Location
         .Include(x => x.Weather)
          .ToListAsync();
        }

        public async Task<Location?> GetById(int? departmentCode)
        {
            // Ensure Context.Location is not null before accessing it
            if (Context.Location == null)
            {
                throw new InvalidOperationException("The Location DbSet is not initialized.");
            }

            // Return the result, explicitly allowing for a null return value
            return await Context.Location.FirstOrDefaultAsync(x => x.DepartmentCode == departmentCode);
        }

        public async Task Add(Location item, ICollection<Weather> weather)
        {
            // Démarrer une activité pour l'insertion dans la base de données
            using var activity = new ActivitySource("WeatherController").StartActivity("Insert Location into DB", ActivityKind.Internal);

            if (activity != null)
            {
                Console.WriteLine($"Current activity found. TraceId: {activity.Context.TraceId}, SpanId: {activity.Context.SpanId}");
            }
            else
            {
                Console.WriteLine("Failed to start new activity. The trace context might not be properly propagated.");
            }

            // Effectuer l'opération d'ajout dans la base de données

            var location = await GetById(item.DepartmentCode);
            if (location == null)
            {
                //item.Weather = null;
                await Context.Location.AddAsync(item);
            }
            else
            {
                foreach (Weather w in weather)
                {
                    w.LocationId = location?.Id;
                }
                await Context.Weather.AddRangeAsync(weather);
            }

            await Context.SaveChangesAsync();
        }
    }
}