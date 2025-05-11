using global::WeatherForecast.Infrastructure.Models;
using global::WeatherForecast.WebApi.Models;
using WeatherForecast.WebApi.Models.Creation;

namespace WeatherForecast.WebApi
{
    namespace WeatherForecast.Application.Mappers
    {
        public static class WeatherForecastDtoExtensions
        {
            public static Location ToLocation(this WeatherForecastForCreationDto dto)
            {
                return new Location
                {
                    Department = dto.Department,
                    DepartmentCode = dto.DepartmentCode,
                    City = dto.City,
                    PostalCode = dto.PostalCode,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    GenerationTimeMs = dto.GenerationtimeMs,
                    UtcOffsetSeconds = dto.UtcOffsetSeconds,
                    Timezone = dto.Timezone,
                    TimezoneAbbreviation = dto.TimezoneAbbreviation,
                    Elevation = dto.Elevation,
                    Weather = dto.CurrentWeather != null
                        ? new List<Infrastructure.Models.Weather>
                        {
                        new Infrastructure.Models.Weather
                        {
                            Time = dto.CurrentWeather.Time,
                            Interval = dto.CurrentWeather.Interval,
                            Temperature = dto.CurrentWeather.Temperature,
                            Windspeed = dto.CurrentWeather.Windspeed,
                            Winddirection = dto.CurrentWeather.Winddirection,
                            IsDay = dto.CurrentWeather.IsDay,
                            Weathercode = dto.CurrentWeather.Weathercode
                        }
                        }
                        : new List<Infrastructure.Models.Weather>(),
                };
            }

            public static WeatherForecastForCreationDto ToDto(this Location location)
            {
                return new WeatherForecastForCreationDto
                {
                    Department = location.Department,
                    DepartmentCode = location.DepartmentCode ?? 0,
                    City = location.City,
                    PostalCode = location.PostalCode ?? 0,
                    Latitude = location.Latitude ?? 0,
                    Longitude = location.Longitude ?? 0,
                    GenerationtimeMs = location.GenerationTimeMs ?? 0,
                    UtcOffsetSeconds = location.UtcOffsetSeconds ?? 0,
                    Timezone = location.Timezone,
                    TimezoneAbbreviation = location.TimezoneAbbreviation,
                    Elevation = location.Elevation ?? 0,
                    CurrentWeather = location.Weather?.FirstOrDefault() is { } w
                        ? new CurrentWeather
                        {
                            Time = w.Time,
                            Interval = w.Interval ?? 0,
                            Temperature = w.Temperature ?? 0,
                            Windspeed = w.Windspeed ?? 0,
                            Winddirection = w.Winddirection ?? 0,
                            IsDay = w.IsDay ?? 0,
                            Weathercode = w.Weathercode ?? 0
                        }
                        : null,
                    //CurrentWeatherUnits = location.WeatherUnits?.FirstOrDefault() is { } wu
                    //    ? new CurrentWeatherUnits
                    //    {
                    //        Time = wu.Time,
                    //        Interval = wu.Interval,
                    //        Temperature = wu.Temperature,
                    //        Windspeed = wu.Windspeed,
                    //        Winddirection = wu.Winddirection,
                    //        IsDay = wu.IsDay,
                    //        Weathercode = wu.Weathercode
                    //    }
                    //    : null
                };
            }

            public static WeatherForecastInListDto ToListDto(this Location location)
            {
                return new WeatherForecastInListDto
                {
                    LocationId = location.Id,
                    Department = location.Department,
                    DepartmentCode = location.DepartmentCode ?? 0,
                    City = location.City,
                    PostalCode = location.PostalCode ?? 0,
                    Latitude = location.Latitude ?? 0,
                    Longitude = location.Longitude ?? 0,
                    GenerationtimeMs = location.GenerationTimeMs ?? 0,
                    UtcOffsetSeconds = location.UtcOffsetSeconds ?? 0,
                    Timezone = location.Timezone,
                    TimezoneAbbreviation = location.TimezoneAbbreviation,
                    Elevation = location.Elevation ?? 0,
                    CurrentWeather = location.Weather.Select(w => new CurrentWeatherInListDto

                    {
                        Time = w.Time,
                        Interval = w.Interval ?? 0,
                        Temperature = w.Temperature ?? 0,
                        Windspeed = w.Windspeed ?? 0,
                        Winddirection = w.Winddirection ?? 0,
                        IsDay = w.IsDay ?? 0,
                        Weathercode = w.Weathercode ?? 0
                    }),
                    WeatherUnits = new CurrentWeatherUnitsInListDto()
                };
            }
        }
    }
}