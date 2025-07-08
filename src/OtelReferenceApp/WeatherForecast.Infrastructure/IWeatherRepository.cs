using WeatherForecast.Infrastructure.Models;

namespace LogisticManagement.Infrastructure.Repositories
{
    public interface IWeatherRepository
    {
        Task Add(Location item, ICollection<Weather> weather);

        Task<IEnumerable<Location>> GetAll();

        Task<Location?> Get(int locationId);
    }
}