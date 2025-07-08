using WeatherForecast.WebApi.Models;
using WeatherForecast.WebApi.Models.Creation;

namespace WeatherForecast.WebApi.Services
{
    public interface IWeatherService
    {
        Task Add(WeatherForecastForCreationDto weatherDto);

        Task<List<WeatherForecastInListDto>> Get();

        Task<WeatherForecastInListDto> Get(int locationId);
    }
}