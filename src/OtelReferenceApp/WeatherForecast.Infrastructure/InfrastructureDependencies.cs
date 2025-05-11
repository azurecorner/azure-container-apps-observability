using LogisticManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeatherForecast.Infrastructure.Models;

namespace WeatherForecast.Infrastructure
{
    public static class InfrastructureDependencies
    {
        public static IServiceCollection RegisterInfrastureDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<WeatherForecastDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DbConnection"));
            });

            services.AddScoped<WeatherForecastDbContext>();

            services.AddScoped<IWeatherRepository, WeatherRepository>();

            return services;
        }
    }
}